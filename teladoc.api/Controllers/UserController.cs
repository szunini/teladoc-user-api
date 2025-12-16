using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using teladoc.domain.Contracts.Services;
using teladoc.domain.DTOs;
using teladoc.domain.Enum;
using teladoc.domain.Exceptions;

namespace teladoc.api.Controllers
{
    /// <summary>
    /// User Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    //This should stay uncomment in real case, just for simplicity I set v1 static
    //[Route("api/v{version:apiVersion}/users")]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET api/user
        /// 
        /// </remarks>
        /// <returns>A list of all users, each including their calculated age.</returns>
        /// <response code="200">Returns the list of users.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all users");
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve. Must be a positive integer.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET api/user/1
        /// 
        /// </remarks>
        /// <returns>The user information, including calculated age, if found.</returns>
        /// <response code="200">Returns the user with the specified ID.</response>
        /// <response code="404">No user exists with the specified ID.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Geting user with id: {id}");

            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("user with id {UserId} not found", id);
                throw new UserNotFoundException(id);
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user with the specified information.
        /// </summary>
        /// <param name="request">The user details to use for creating the new user. Must not be null and must satisfy all validation requirements.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST api/user
        /// 
        ///     {
        ///         "firstName": "Juan",
        ///         "lastName": "Pérez",
        ///         "email": "juan.perez@example.com",
        ///         "dateOfBirth": "1990-05-15",
        ///         "nickName": "JP",
        ///         "friendCount": 5
        ///     }
        /// 
        /// Business rules:
        /// - Email must be unique.
        /// - User must be at least 18 years old.
        /// - All required fields must be provided and valid.
        /// </remarks>
        /// <returns>The created user, including their assigned ID and calculated age.</returns>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Validation failed (e.g., user is under 18, email already exists, or invalid input).</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid User Request {request}");
                return ValidationProblem(ModelState);
            }

            var createdUser = await _userService.CreateUserAsync(request, cancellationToken);

            _logger.LogInformation("User created con ID: {UserId}", createdUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Updates an existing user by replacing all fields.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="request">The updated user data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// 
        /// Sample Request:
        /// 
        ///     PUT api/user/1
        ///     
        ///     {
        ///         "firstName": "Juan",
        ///         "lastName": "Pérez",
        ///         "email": "juan.perez@example.com",
        ///         "dateOfBirth": "1990-05-15",
        ///         "nickName": "JP",
        ///         "friendCount": 10
        ///     }
        ///     
        /// </remarks>
        /// <returns>The updated user.</returns>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest user, CancellationToken cancellationToken)
        {
            var current = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (current is null)
                return NotFound();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.ReplaceUserAsync(id, user, cancellationToken);

            return result.Type switch
            {
                PatchUserResultEnum.NotFound => NotFound(),
                PatchUserResultEnum.ValidationError =>
                    ValidationProblem(new ValidationProblemDetails(result.Errors!)),
                _ => NoContent()
            };
        }

        /// <summary>
        /// Partially updates an existing user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to patch.</param>
        /// <param name="patch">The fields to update.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// 
        /// Sample Request:
        /// 
        ///     PATCH api/user/1
        ///     
        ///     {
        ///         "nickName": "NuevoNick"
        ///     }
        ///     
        /// </remarks>
        /// <returns>The patched user.</returns>
        /// <response code="200">User patched successfully.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">User not found.</response>
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateUserCommand> patch, CancellationToken cancellationToken)
        {
            var current = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (current is null)
                return NotFound();

            var command = new UpdateUserCommand
            {
                FirstName = current.FirstName,
                LastName = current.LastName,
                Nickname = current.NickName,
                FriendCount = current.FriendCount,
                DateOfBirth = current.DateOfBirth,
                Email = current.Email
            };

            patch.ApplyTo(command, ModelState);

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.UpdateUserAsync(id, command, cancellationToken);

            return result.Type switch
            {
                PatchUserResultEnum.NotFound => NotFound(),
                PatchUserResultEnum.ValidationError =>
                    ValidationProblem(new ValidationProblemDetails(result.Errors!)),
                _ => NoContent()
            };
        }


        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// 
        /// Sample Request:
        /// 
        ///     DELETE api/user/1
        ///     
        /// </remarks>
        /// <returns>No content if deleted successfully.</returns>
        /// <response code="204">User deleted successfully.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeleteUserAsync(id, cancellationToken);

            return result switch
            {
                DeleteUserResultEnum.NotFound => NotFound(),
                _ => NoContent()
            };
        }
    }
}
