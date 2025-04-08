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

namespace Tiger.Stripes.Instrumentation;

/// <summary>Represents the construction of an ARN with a version.</summary>
readonly ref struct VersionedArn
{
    const char Separator = ':';
    const string Arn = "arn";
    const string Lambda = "lambda";
    const string Function = "function";

    readonly ReadOnlySpan<char> _partition;
    readonly ReadOnlySpan<char> _region;
    readonly ReadOnlySpan<char> _resource;
    readonly ReadOnlySpan<char> _version;

    /// <summary>Initializes a new instance of the <see cref="VersionedArn"/> struct.</summary>
    /// <param name="partition">The partition of the identified resource.</param>
    /// <param name="region">The region of the identified resource.</param>
    /// <param name="accountId">The account ID of the identified resource.</param>
    /// <param name="resource">The name of the identified resource.</param>
    /// <param name="version">The version of the identified resource.</param>
    VersionedArn(
        ReadOnlySpan<char> partition,
        ReadOnlySpan<char> region,
        ReadOnlySpan<char> accountId,
        ReadOnlySpan<char> resource,
        ReadOnlySpan<char> version)
    {
        _partition = partition;
        _region = region;
        AccountId = new(accountId);
        _resource = resource;
        _version = version;
    }

    /// <summary>Gets the Account ID from this ARN.</summary>
    public string AccountId { get; }

    /// <summary>
    /// Creates a new <see cref="VersionedArn"/> for the current execution from the specified <typeparamref name="TLambdaContext"/>.
    /// </summary>
    /// <typeparam name="TLambdaContext">The type of the Lambda context.</typeparam>
    /// <param name="context">The Lambda context from which to create the <see cref="VersionedArn"/>.</param>
    /// <returns>A representation of the current execution.</returns>
    public static VersionedArn Create<TLambdaContext>(TLambdaContext context)
        where TLambdaContext : ILambdaContext
    {
        ReadOnlySpan<char> partition = [], region = [], accountId = [], resource = [];
        var index = 0;
        var ifa = context.InvokedFunctionArn.AsSpan();
        foreach (var range in ifa.Split(Separator))
        {
            // arn:<partition>:lambda:<region>:<account-id>:function:<resource-name>
            // 0   1           2      3        4            5        6
            switch (index)
            {
                case 1: // partition
                    partition = ifa[range];
                    break;
                case 3: // region
                    region = ifa[range];
                    break;
                case 4: // account-id
                    accountId = ifa[range];
                    break;
                case 6: // function-name
                    resource = ifa[range];
                    break;
                default:
                    _ = ifa[range];
                    break;
            }

            index += 1;
        }

        return new(partition, region, accountId, resource, context.FunctionVersion);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var totalLength = Arn.Length + _partition.Length + Lambda.Length + _region.Length + AccountId.Length + Function.Length + _resource.Length + _version.Length + 7;
        return string.Create(totalLength, this, static (span, state) =>
        {
            var index = 0;

            Apply(Arn, span, ref index);
            Apply(state._partition, span, ref index);
            Apply(Lambda, span, ref index);
            Apply(state._region, span, ref index);
            Apply(state.AccountId, span, ref index);
            Apply(Function, span, ref index);
            Apply(state._resource, span, ref index);
            state._version.CopyTo(span[index..]);

            static void Apply(ReadOnlySpan<char> source, Span<char> destination, ref int index)
            {
                source.CopyTo(destination[index..]);
                index += source.Length;
                destination[index] = Separator;
                index += 1;
            }
        });
    }
}
