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

using static Tiger.Stripes.Experimental.InvocationMapping;

namespace Tiger.Stripes.Experimental;

/// <summary>Compares instances of the <see cref="InvocationMapping"/> class for equality.</summary>
sealed class InvocationMappingComparer
    : IEqualityComparer<InvocationMapping>
{
    /// <summary>The singleton instance of this comparer.</summary>
    public static readonly InvocationMappingComparer Instance = new();

    /// <inheritdoc/>
    public bool Equals(InvocationMapping a, InvocationMapping b) => SignatureEquals(a, b);

    /// <inheritdoc/>
    public int GetHashCode(InvocationMapping endpoint) => GetSignatureHashCode(endpoint);
}
