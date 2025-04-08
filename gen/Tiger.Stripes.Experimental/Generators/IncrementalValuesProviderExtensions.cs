// <copyright file="IncrementalValuesProviderExtensions.cs" company="Cimpress plc">
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
// </copyright>

namespace Tiger.Stripes.Experimental;

/// <summary>Extensions to the functionality of the <see cref="IncrementalValuesProvider{T}"/> class.</summary>
static class IncrementalValuesProviderExtensions
{
    /// <summary>Groups the values of the source by the result of the given selector.</summary>
    /// <typeparam name="TSource">The type of the source values.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the groups.</typeparam>
    /// <param name="source">The source of values to group.</param>
    /// <param name="sourceToElementTransform">A function to transform the source values into elements.</param>
    /// <param name="comparer">The comparer to use to compare the source values.</param>
    /// <returns>The grouped values.</returns>
    public static IncrementalValuesProvider<(TSource Source, int Index, ImmutableArray<TElement> Elements)> GroupWith<TSource, TElement>(
        this IncrementalValuesProvider<TSource> source,
        Func<TSource, TElement> sourceToElementTransform,
        IEqualityComparer<TSource> comparer)
        where TSource : notnull => source.Collect().SelectMany((values, _) => values
            .GroupBy(Id, sourceToElementTransform, comparer)
            .Select(static (g, i) => (g.Key, Index: i, g.ToImmutableArray())));

    static T Id<T>(T t) => t;
}
