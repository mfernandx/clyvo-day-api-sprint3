using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;

namespace ClyvoDayApiWeb.UnitTests.Domain
{
    public class CareEventTests
    {
        private static CareEvent CreateCareEvent()
        {
            return new CareEvent(
                petId: 1,
                typeEvent: "Vacinação",
                description: "Vacina B12",
                eventDate: DateTime.UtcNow.AddDays(10),
                observations: "Nenhuma"
            );
        }

        [Fact]
        public void Complete_EventoAgendado_DeveAlterarStatusParaCompleted()
        {
            // Arrange
            var careEvent = CreateCareEvent();

            // Act
            careEvent.Complete();

            // Assert
            Assert.Equal(EnumCareEventStatus.Completed,careEvent.Status);
        }

        [Fact]
        public void Complete_EventoJaConcluido_DeveLancarInvalidOperationException()
        {
            // Arrange
            var careEvent = CreateCareEvent();

            careEvent.Complete();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => careEvent.Complete());

            // Assert
            Assert.Equal("O evento já foi concluído.",exception.Message);
        }

        [Fact]
        public void Complete_EventoCancelado_DeveLancarInvalidOperationException()
        {
            // Arrange
            var careEvent = CreateCareEvent();

            careEvent.Cancel();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => careEvent.Complete());

            // Assert
            Assert.Equal("Um evento cancelado não pode ser concluído.", exception.Message);
        }

        [Fact]
        public void Cancel_EventoAgendado_DeveAlterarStatusParaCancelled()
        {
            // Arrange
            var careEvent = CreateCareEvent();

            // Act
            careEvent.Cancel();

            // Assert
            Assert.Equal(EnumCareEventStatus.Cancelled, careEvent.Status);
        }

        [Fact]
        public void Cancel_EventoConcluido_DeveLancarInvalidOperationException()
        {
            // Arrange
            var careEvent = CreateCareEvent();

            careEvent.Complete();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => careEvent.Cancel());

            // Assert
            Assert.Equal("Um evento concluído não pode ser cancelado.",exception.Message);
        }
    }
}
