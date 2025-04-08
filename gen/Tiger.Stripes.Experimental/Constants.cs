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

namespace Tiger.Stripes.Experimental;

/// <summary>Constants.</summary>
static class Constants
{
    /// <summary>The name of the method that is used to map invocations.</summary>
    public const string MapInvokeMethodName = "MapInvoke";

    /// <summary>The acceptable method names for a service type.</summary>
    public static readonly FrozenSet<string> AcceptableMethodNames = FrozenSet.ToFrozenSet(["HandleAsync", "Handle"], SC.Ordinal);
}
