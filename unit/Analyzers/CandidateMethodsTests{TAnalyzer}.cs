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

/// <summary>Tests for analysis by signature.</summary>
/// <typeparam name="TAnalyzer">The analyzer to test.</typeparam>
public abstract class CandidateMethodsTests<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    [Fact(DisplayName = "An empty source code file produces no diagnostic.")]
    public Task EmptySourceCode_Empty() => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(string.Empty);

    [Theory(DisplayName = "A correctly formed class produces no diagnostic.")]
    [ClassData(typeof(NoDiagnostic))]
    public Task SingleCandidateMethodBySignature_Empty(string source)
    {
        var context = new AnalyzerTest<TAnalyzer>(source);
        return context.RunAsync(TestContext.Current.CancellationToken);
    }
}
