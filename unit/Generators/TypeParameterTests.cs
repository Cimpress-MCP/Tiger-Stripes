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

public sealed class TypeParameterTests
{
    [Theory(DisplayName = "Valid invocation mappings generate the expected code.")]
    [ClassData(typeof(SuccessfulGeneration))]
    public static async Task ValidInvocation_Expected(string source, string expected)
    {
        var context = new SourceGeneratorTest<TypeParameterGenerator>(source, expected);
        await context.RunAsync(TestContext.Current.CancellationToken);
    }
}
