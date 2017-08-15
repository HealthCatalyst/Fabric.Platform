﻿using System.Linq;
using System.Threading.Tasks;
using LibOwin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Fabric.Platform.Auth
{
    public class AuthorizationMiddleware
    {
        public static AppFunc Inject(AppFunc next, string[] requiredScopes, string[] allowedPaths = null)
        {
            return env =>
            {
                var ctx = new OwinContext(env);
                if (ctx.Request.Method == "OPTIONS") return next(env);

                if (allowedPaths != null && allowedPaths.Contains(ctx.Request.Path.Value)) return next(env);

                var principal = ctx.Request.User;
                if (principal != null)
                {
                    if (requiredScopes.Any(requiredScope => principal.HasClaim("scope", requiredScope)))
                    {
                        return next(env);
                    }
                }
                ctx.Response.StatusCode = 403;
                return Task.FromResult(0);
            };
        }
    }
}
