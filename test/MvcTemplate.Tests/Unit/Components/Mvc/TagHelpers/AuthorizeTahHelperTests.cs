﻿using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using MvcTemplate.Components.Mvc;
using MvcTemplate.Components.Security;
using NSubstitute;
using System;
using Xunit;

namespace MvcTemplate.Tests.Unit.Components.Mvc
{
    public class AuthorizeTahHelperTests : IDisposable
    {
        private AuthorizeTagHelper helper;
        private TagHelperOutput output;

        public AuthorizeTahHelperTests()
        {
            helper = new AuthorizeTagHelper();
            helper.ViewContext = HtmlHelperFactory.CreateHtmlHelper().ViewContext;
            output = new TagHelperOutput("authorize", new TagHelperAttributeList());

            Authorization.Provider = Substitute.For<IAuthorizationProvider>();
        }
        public void Dispose()
        {
            Authorization.Provider = null;
        }

        #region Method: Process(TagHelperContext context, TagHelperOutput output)

        [Fact]
        public void Process_OnNullAuthorizationProviderRemovesWrappingTag()
        {
            output.Content.SetContent("Content");
            Authorization.Provider = null;
            output.TagName = "TagName";

            helper.Process(null, output);

            Assert.Equal("Content", output.Content.GetContent());
            Assert.Null(output.TagName);
        }

        [Theory]
        [InlineData("A", "B", "C", "D", "E", "F", "A", "B", "C")]
        [InlineData(null, null, null, "A", "B", "C", "A", "B", "C")]
        [InlineData(null, null, null, null, null, null, null, null, null)]
        public void Process_OnNotAuthorizedSurpressesTheOutput(
            String area, String controller, String action,
            String routeArea, String routeController, String routeAction,
            String authArea, String authController, String authAction)
        {
            Authorization.Provider.IsAuthorizedFor("Acc", Arg.Any<String>(), Arg.Any<String>(), Arg.Any<String>()).Returns(true);
            Authorization.Provider.IsAuthorizedFor("Acc", authArea, authController, authAction).Returns(false);
            helper.ViewContext.HttpContext.User.Identity.Name.Returns("Acc");

            helper.ViewContext.RouteData.Values["controller"] = routeController;
            helper.ViewContext.RouteData.Values["action"] = routeAction;
            helper.ViewContext.RouteData.Values["area"] = routeArea;

            output.PostContent.SetContent("PostContent");
            output.PostElement.SetContent("PostElement");
            output.PreContent.SetContent("PreContent");
            output.PreElement.SetContent("PreElement");
            output.Content.SetContent("Content");
            output.TagName = "TagName";

            helper.Controller = controller;
            helper.Action = action;
            helper.Area = area;

            helper.Process(null, output);

            Assert.Empty(output.PostContent.GetContent());
            Assert.Empty(output.PostElement.GetContent());
            Assert.Empty(output.PreContent.GetContent());
            Assert.Empty(output.PreElement.GetContent());
            Assert.Empty(output.Content.GetContent());
            Assert.Null(output.TagName);
        }

        [Theory]
        [InlineData("A", "B", "C", "D", "E", "F", "A", "B", "C")]
        [InlineData(null, null, null, "A", "B", "C", "A", "B", "C")]
        [InlineData(null, null, null, null, null, null, null, null, null)]
        public void Process_OnAuthorizedRemovesWrappingTag(
            String area, String controller, String action,
            String routeArea, String routeController, String routeAction,
            String authArea, String authController, String authAction)
        {
            Authorization.Provider.IsAuthorizedFor("Acc", Arg.Any<String>(), Arg.Any<String>(), Arg.Any<String>()).Returns(false);
            Authorization.Provider.IsAuthorizedFor("Acc", authArea, authController, authAction).Returns(true);
            helper.ViewContext.HttpContext.User.Identity.Name.Returns("Acc");

            helper.ViewContext.RouteData.Values["controller"] = routeController;
            helper.ViewContext.RouteData.Values["action"] = routeAction;
            helper.ViewContext.RouteData.Values["area"] = routeArea;

            output.PostContent.SetContent("PostContent");
            output.PostElement.SetContent("PostElement");
            output.PreContent.SetContent("PreContent");
            output.PreElement.SetContent("PreElement");
            output.Content.SetContent("Content");
            output.TagName = "TagName";

            helper.Controller = controller;
            helper.Action = action;
            helper.Area = area;

            helper.Process(null, output);

            Assert.Equal("PostContent", output.PostContent.GetContent());
            Assert.Equal("PostElement", output.PostElement.GetContent());
            Assert.Equal("PreContent", output.PreContent.GetContent());
            Assert.Equal("PreElement", output.PreElement.GetContent());
            Assert.Equal("Content", output.Content.GetContent());
            Assert.Null(output.TagName);
        }

        #endregion
    }
}