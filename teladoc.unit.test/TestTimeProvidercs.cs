using System;
using System.Collections.Generic;
using System.Text;

namespace teladoc.unit.test
{
    public sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public TestTimeProvider(DateTimeOffset now) => _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }

}
