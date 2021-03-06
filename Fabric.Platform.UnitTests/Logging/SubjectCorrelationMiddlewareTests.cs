﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LibOwin;
using Xunit;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using Fabric.Platform.Logging;
using Fabric.Platform.Shared;
using Fabric.Platform.UnitTests.Mocks;

namespace Fabric.Platform.UnitTests.Logging
{
    public class SubjectCorrelationMiddlewareTests
    {
        private readonly AppFunc _noOp = env => Task.FromResult(0);
        private const string TestUser = "testuser";
        private const string UnknownUser = "unknown";

        [Fact]
        public void UserIdMiddleware_Inject_AddsUserIdFromContext()
        {
            //Arrange
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/"),
                    Method = "GET",
                    User = new TestPrincipal(new Claim(SubjectCorrelationMiddleware.SubClaim, TestUser))
                }
            };

            //Act
            var pipeline = SubjectCorrelationMiddleware.Inject(_noOp);
            pipeline(ctx.Environment);

            //Assert
            var actualUserId = ctx.Environment[Constants.FabricHeaders.SubjectNameHeader];
            Assert.Equal(TestUser, actualUserId);

        }

        [Fact]
        public void UserIdMiddleware_Inject_DoesNotAddUserIdWhenNotPresent()
        {
            //Arrange
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/"),
                    Method = "GET"
                }
            };

            //Act
            var pipeline = SubjectCorrelationMiddleware.Inject(_noOp);
            pipeline(ctx.Environment);

            //Assert
            var actualUserId = ctx.Environment[Constants.FabricHeaders.SubjectNameHeader];
            Assert.Equal(UnknownUser, actualUserId);

        }

        [Fact]
        public void UserIdMiddleware_Inject_DoesNotAddUserIdWhenSubIsNotPresent()
        {
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/"),
                    Method = "GET",
                    User = new TestPrincipal()
                }
            };

            var pipeline = SubjectCorrelationMiddleware.Inject(_noOp);
            pipeline(ctx.Environment);

            //Assert
            var actualUserId = ctx.Environment[Constants.FabricHeaders.SubjectNameHeader];
            Assert.Equal(UnknownUser, actualUserId);
        }

        [Fact]
        public void UserIdMiddleware_Inject_UsesSubClaimWhenPresent()
        {
            var subClaim = "testuser";
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/"),
                    Method = "GET",
                    User = new TestPrincipal(new Claim(SubjectCorrelationMiddleware.SubClaim, subClaim))
                }
            };

            var pipeline = SubjectCorrelationMiddleware.Inject(_noOp);
            pipeline(ctx.Environment);

            //Assert
            var actualUserId = ctx.Environment[Constants.FabricHeaders.SubjectNameHeader];
            Assert.Equal(subClaim, actualUserId);
        }

        [Fact]
        public void UserIdCorrelationMiddleware_Inject_UsesExistingUserIdWhenPresent()
        {
            //Arrange
            var ctx = new OwinContext
            {
                Request =
                {
                    Scheme = LibOwin.Infrastructure.Constants.Https,
                    Path = new PathString("/"),
                    Method = "GET",
                }
            };
            ctx.Request.Headers.Append(Constants.FabricHeaders.SubjectNameHeader, TestUser);

            //Act
            var pipeline = SubjectCorrelationMiddleware.Inject(_noOp);
            pipeline(ctx.Environment);

            //Assert
            var actualUserId = ctx.Environment[Constants.FabricHeaders.SubjectNameHeader];
            Assert.Equal(TestUser, actualUserId);
        }
    }
}
