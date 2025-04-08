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

/// <summary>Registry of Lambda bootstrap handlers.</summary>
sealed class LambdaBootstrapHandlerRegistry
{
    readonly HandlerMap _handlers = new(Ordinal);

    /// <summary>Gets or sets the handler with the specified name.</summary>
    /// <param name="name">The name of the handler to get or set.</param>
    /// <returns>The handler with the specified name.</returns>
    /// <exception cref="InvalidOperationException">No handler is registered for the specified name.</exception>
    public LambdaBootstrapHandler this[string name]
    {
        get
        {
            if (!_handlers.TryGetValue(name, out var handler))
            {
                var message = name is DefaultHandlerName
                    ? "No default handler is registered!"
                    : $"No handler is registered for name '{name}'!";
                NoSuchHandler(message, _handlers.Keys);
            }

            return handler;
        }
    }

    /// <summary>Adds a handler with the specified name.</summary>
    /// <param name="name">The name of the handler to add.</param>
    /// <param name="handler">The handler to add.</param>
    public void Add(string name, LambdaBootstrapHandler handler) => _handlers.Add(name, handler);

    /// <summary>Throws a new <see cref="InvalidOperationException"/> indicating that no handler was found.</summary>
    /// <param name="message">The message to include in the exception.</param>
    /// <param name="allHandlers">The collection of all handlers.</param>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [DoesNotReturn]
    static void NoSuchHandler<TCollection>(string message, TCollection allHandlers)
        where TCollection : IEnumerable<string> => throw new InvalidOperationException($"""
{message} (Known handlers are: {string.Join(", ", allHandlers.Select(static k => $"'{k}'"))}.)
""");
}
