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

namespace Tiger.Stripes;

/// <summary>Extensions to the functionality of the <see cref="IInvocationBuilder"/> interface.</summary>
public static partial class InvocationBuilderExtensions
{
    static readonly Pipe s_outputPipe = new(new(useSynchronizationContext: false));
    static readonly Utf8JsonWriter s_jsonWriter = new(s_outputPipe.Writer);

    /*
        /// <summary>Maps a Lambda Function invocation to the specified parameters.</summary>
        /// <typeparam name="TService">The type of the service which is the Lambda Function handler.</typeparam>
        /// <param name="builder">The invocation builder to which to add the handler.</param>
        /// <param name="name">The name to which to map the provided handler.</param>
        /// <returns>The invocation builder for further customization.</returns>
        public static IInvocationBuilder MapInvoke<[DynamicallyAccessedMembers(PublicConstructors | PublicMethods)] TService>(
            this IInvocationBuilder builder,
            string name)
            where TService : class => throw new NotImplementedException("This method is not intended to be called directly. It is used by the code generator.");
    */

    [MethodImpl(AggressiveInlining)]
    static TDependency GetSpecializedService<TDependency>(this IServiceProvider serviceProvider, IExtendedLambdaContext context)
        where TDependency : notnull
    {
        /* note(cosborn)
         * Whether or not the ordering here is actually important, it feels like it is,
         * so let's go over the reasoning in detail. First are the pseudo-dependencies:
         * these are things that are not actually in the DI container, but are provided
         * by the Lambda runtime. These change with each request (even the logger; it's
         * associated with the request ID for the log messages), so they can be neither
         * resolved nor specialized. Then comes a check to see if the dependency can be
         * resolved at all. If someone has already registered in the DI container, say,
         * a bare `ILogger`, then we should respect that. Then come the specializations
         * for which we can do something helpful or smart. Finally, we fall back to the
         * original `GetRequiredService` method so that expected exceptions get thrown.
         */

        if (typeof(TDependency) == typeof(ILambdaContext) || typeof(TDependency) == typeof(IExtendedLambdaContext))
        {
            return (TDependency)context;
        }

        if (serviceProvider.GetService<TDependency>() is { } dep)
        {
            return dep;
        }

        /* note(cosborn)
         * A bare `ILoggger` doesn't get added to the DI container – only `ILogger<T>`.
         * We predict that handlers will frequently be static methods defined in static
         * classes, so we allow it to be resolved and given a reasonable category name.
         */
        if (typeof(TDependency) == typeof(ILogger))
        {
            return context.Name is DefaultHandlerName
                ? (TDependency)context.Logger
                : (TDependency)serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(context.Name);
        }

        // note(cosborn) Retain existing exception behavior if dependency is not found.
        return serviceProvider.GetRequiredService<TDependency>();
    }
}
