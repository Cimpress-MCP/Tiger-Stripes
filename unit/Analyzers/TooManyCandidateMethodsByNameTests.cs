// Copyright 2024 Cimpress plc
//
// Licensed under the Apache License, Version 2.0 (the "License") –
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Test;

public sealed class TooManyCandidateMethodsByNameTests
    : CandidateMethodsTests<TooManyCandidateMethodsByNameAnalyzer>
{
    [Theory(DisplayName = "A class with too many candidate methods produces TS0003.")]
    [InlineData(/* lang=c#-test */ """
using System;
using System.Threading;
using System.Threading.Tasks;
using Tiger.Stripes;

_ = ((IInvocationBuilder)null!).MapInvoke<[|Service|]>("a", null!);

sealed class Service
{
    public async ValueTask<int> HandleAsync(int value, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return value;
    }

    public async ValueTask<string> HandleAsync(string value, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return value;
    }
}
""")]
    [InlineData(/* lang=c#-test */ """
using System;
using System.Threading;
using System.Threading.Tasks;
using Tiger.Stripes;

_ = ((IInvocationBuilder)null!).MapInvoke<[|Service|]>("a", null!);

sealed class Service
{
    public async Task<int> HandleAsync(int value, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return value;
    }

    public async Task<string> HandleAsync(string value, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return value;
    }
}
""")]
    [InlineData(/* lang=c#-test */ """
using System;
using System.Threading;
using System.Threading.Tasks;
using Tiger.Stripes;

_ = ((IInvocationBuilder)null!).MapInvoke<[|Service|]>("a", null!);

sealed class Service
{
    public int Handle(int value) => value;

    public string Handle(string value) => value;
}
""")]
    [InlineData(/* lang=c#-test */ """
using System;
using System.Threading;
using System.Threading.Tasks;
using Tiger.Stripes;

_ = ((IInvocationBuilder)null!).MapInvoke<[|Service|]>("a", null!);

sealed class Service
{
    public async Task<int> HandleAsync(int value, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        return value;
    }

    public string Handle(string value) => value;
}
""")]
    public Task TooManyCandidateMethods(string source)
    {
        var context = new AnalyzerTest<TooManyCandidateMethodsByNameAnalyzer>(source);
        return context.RunAsync(TestContext.Current.CancellationToken);
    }
}
