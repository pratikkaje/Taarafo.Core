﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using Taarafo.Core.Models.GroupPosts;
using Xunit;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.GroupPosts
{
    public partial class GroupPostServiceTests
    {
        [Fact]
        public async Task ShouldRemoveGroupPostByIdAsync()
        {
            // given
            Guid randomId = Guid.NewGuid();
            Guid inputGroupPostId = randomId;
            GroupPost randomGroupPost = CreateRandomGroupPost();
            GroupPost storageGroupPost = randomGroupPost;
            GroupPost expectedInputGroupPost = storageGroupPost;
            GroupPost deletedGroupPost = expectedInputGroupPost;
            GroupPost expectedGroupPost = deletedGroupPost.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectGroupPostByIdAsync(inputGroupPostId))
                    .ReturnsAsync(storageGroupPost);

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteGroupPostAsync(expectedInputGroupPost))
                    .ReturnsAsync(deletedGroupPost);

            // when
            GroupPost actualGroupPost = await this.groupPostService
                .RemoveGroupPostByIdAsync(inputGroupPostId);

            // then
            actualGroupPost.Should().BeEquivalentTo(expectedGroupPost);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectGroupPostByIdAsync(inputGroupPostId), Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteGroupPostAsync(expectedInputGroupPost), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
