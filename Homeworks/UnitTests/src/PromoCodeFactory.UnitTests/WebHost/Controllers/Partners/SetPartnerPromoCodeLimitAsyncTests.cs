using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnerRepositoryMock;
        private readonly PartnersController _sut;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _partnerRepositoryMock = new Mock<IRepository<Partner>>();
            _sut = new PartnersController(_partnerRepositoryMock.Object);
        }

        private static Partner CreatePartner(Guid id, bool isActive, int issuedPromoCodes, List<PartnerPromoCodeLimit> limits = null)
        {
            return new Partner
            {
                Id = id,
                IsActive = isActive,
                NumberIssuedPromoCodes = issuedPromoCodes,
                PartnerLimits = limits ?? new List<PartnerPromoCodeLimit>()
            };
        }

        private static PartnerPromoCodeLimit CreateLimit(Guid id, int limit, DateTime? endDate = null, DateTime? cancelDate = null)
        {
            return new PartnerPromoCodeLimit
            {
                Id = id,
                Limit = limit,
                CreateDate = DateTime.Now.AddDays(-1),
                EndDate = endDate,
                CancelDate = cancelDate
            };
        }

        /// <summary>
        /// Проверяет, что если партнер не найден, возвращается ошибка 404 (NotFound).
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerDoesNotExist_ShouldReturnNotFound()
        {
            var partnerId = Guid.NewGuid();
            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync((Partner)null);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(7)
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        /// Проверяет, что если партнер заблокирован (IsActive = false), возвращается ошибка 400 (BadRequest).
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsInactive_ShouldReturnBadRequest()
        {
            var partnerId = Guid.NewGuid();
            var inactivePartner = CreatePartner(partnerId, false, 0);

            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(inactivePartner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(7)
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Данный партнер не активен");
        }

        /// <summary>
        /// Проверяет, что при установке нового лимита старый лимит отключается и счетчик выданных промокодов сбрасывается в 0.
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ActiveLimitExists_ShouldResetIssuedCodesAndCancelOldLimit()
        {
            var partnerId = Guid.NewGuid();
            var oldLimit = CreateLimit(Guid.NewGuid(), 10);
            var partner = CreatePartner(partnerId, true, 25, new List<PartnerPromoCodeLimit> { oldLimit });

            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(7)
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(PartnersController.GetPartnerLimitAsync));

            partner.NumberIssuedPromoCodes.Should().Be(0);
            oldLimit.CancelDate.Should().NotBeNull();

            _partnerRepositoryMock.Verify(r => r.UpdateAsync(partner), Times.Once);
        }

        /// <summary>
        /// Проверяет, что счетчик выданных промокодов не сбрасывается, если предыдущий лимит истек.
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ExpiredLimitExists_ShouldNotResetIssuedCodes()
        {
            var partnerId = Guid.NewGuid();
            var expiredLimit = CreateLimit(Guid.NewGuid(), 10, endDate: DateTime.Now.AddDays(-2));
            var partner = CreatePartner(partnerId, true, 25, new List<PartnerPromoCodeLimit> { expiredLimit });

            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(7)
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<CreatedAtActionResult>();

            partner.NumberIssuedPromoCodes.Should().Be(25);
            _partnerRepositoryMock.Verify(r => r.UpdateAsync(partner), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при попытке установить некорректный лимит (<= 0) возвращается ошибка 400 (BadRequest).
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsNotPositive_ShouldReturnBadRequest(int invalidLimit)
        {
            var partnerId = Guid.NewGuid();
            var partner = CreatePartner(partnerId, true, 0);

            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = invalidLimit
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Лимит должен быть больше 0");
        }

        /// <summary>
        /// Проверяет, что новый лимит добавляется и корректно сохраняется в базе данных.
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WhenCalled_ShouldAddNewLimitAndReturnCreated()
        {
            var partnerId = Guid.NewGuid();
            var partner = CreatePartner(partnerId, true, 0);

            _partnerRepositoryMock
                .Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(5)
            };

            var result = await _sut.SetPartnerPromoCodeLimitAsync(partnerId, request);

            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(PartnersController.GetPartnerLimitAsync));

            partner.PartnerLimits.Should().HaveCount(1);

            var newLimit = partner.PartnerLimits.Single();
            newLimit.Limit.Should().Be(100);
            newLimit.EndDate.Should().BeCloseTo(request.EndDate, TimeSpan.FromSeconds(2));
            newLimit.CreateDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            newLimit.CancelDate.Should().BeNull();

            _partnerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Partner>()), Times.Once);

            created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(partner.Id);
            created.RouteValues.Should().ContainKey("limitId").WhoseValue.Should().Be(newLimit.Id);
        }
    }
}
