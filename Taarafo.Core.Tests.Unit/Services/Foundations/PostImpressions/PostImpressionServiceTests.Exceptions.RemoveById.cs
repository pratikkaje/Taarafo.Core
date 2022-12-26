﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Taarafo.Core.Models.Comments.Exceptions;
using Taarafo.Core.Models.PostImpressions;
using Taarafo.Core.Models.PostImpressions.Exceptions;
using Xunit;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.PostImpressions
{
    public partial class PostImpressionServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnDeleteWhenExceptionOccursAndLogItAsync()
        {
            //given
            Guid somePostId = Guid.NewGuid();
            Guid someProfileId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedPostImpressionServiceException =
                new FailedPostImpressionServiceException(serviceException);

            var expectedPostImpressionServiceException =
                new PostImpressionServiceException(failedPostImpressionServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectPostImpressionByIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>())).ThrowsAsync(serviceException);

            //when
            ValueTask<PostImpression> deletePostImpressionTask =
                 this.postImpressionService.RemovePostImpressionByIdAsync(somePostId, someProfileId);

            PostImpressionServiceException actualPostImpressionServiceException =
                await Assert.ThrowsAsync<PostImpressionServiceException>(
                    deletePostImpressionTask.AsTask);

            //then
            actualPostImpressionServiceException.Should().BeEquivalentTo(
                expectedPostImpressionServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectPostImpressionByIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedPostImpressionServiceException))), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnDeleteWhenSqlExceptionOccursAndLogItAsync()
        {
            //given
            Guid somePostId = Guid.NewGuid();
            Guid someProfileId = Guid.NewGuid();
            SqlException sqlException = GetSqlException();

            var failedPostImpressionStorageException =
                new FailedPostImpressionStorageException(sqlException);

            var expectedPostImpressionDependencyException =
                new PostImpressionDependencyException(failedPostImpressionStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectPostImpressionByIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>())).ThrowsAsync(sqlException);

            //when
            ValueTask<PostImpression> deletePostImpressionTask =
                this.postImpressionService.RemovePostImpressionByIdAsync(somePostId, someProfileId);

            PostImpressionDependencyException actualPostImpressionDependencyException =
                await Assert.ThrowsAsync<PostImpressionDependencyException>(
                    deletePostImpressionTask.AsTask);

            //then
            actualPostImpressionDependencyException.Should().BeEquivalentTo(
                expectedPostImpressionDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectPostImpressionByIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedPostImpressionDependencyException))), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationOnRemoveIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            //given
            Guid somePostId= Guid.NewGuid();
            Guid someProfileId= Guid.NewGuid();

            var databaseUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedPostImpressionException =
                new LockedPostImpressionException(databaseUpdateConcurrencyException);

            var expectedPostImpressionDependencyValidationException =
                new PostImpressionDependencyValidationException(lockedPostImpressionException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectPostImpressionByIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>()))
                        .ThrowsAsync(databaseUpdateConcurrencyException);

            //when
            ValueTask<PostImpression> removePostImpressionByIdTask =
                this.postImpressionService.RemovePostImpressionByIdAsync(somePostId, someProfileId);

            PostImpressionDependencyValidationException actualPostImpressionDependencyValidationException =
                await Assert.ThrowsAsync<PostImpressionDependencyValidationException>(
                    removePostImpressionByIdTask.AsTask);

            //then
            actualPostImpressionDependencyValidationException.Should().BeEquivalentTo(
                expectedPostImpressionDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
               broker.SelectPostImpressionByIdsAsync(
                   It.IsAny<Guid>(),
                   It.IsAny<Guid>()), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedPostImpressionDependencyValidationException))), Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeletePostImpressionAsync(It.IsAny<PostImpression>()), Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
