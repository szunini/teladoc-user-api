using teladoc.domain.Enum;

namespace teladoc.domain.Entities
{
    public record PatchUserResult(
         PatchUserResultEnum Type,
         Dictionary<string, string[]>? Errors = null
     );
}
