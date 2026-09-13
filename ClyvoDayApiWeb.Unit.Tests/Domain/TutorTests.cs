using ClyvoDayApiWeb.Domain.Enums;
using ClyvoDayApiWeb.Domain.Models;

namespace ClyvoDayApiWeb.Unit.Tests.Domain
{
    public class TutorTests
    {
        private static Tutor CreateTutor()
        {
            return new Tutor(
                fullName: "Maria Silva",
                email: "maria@email.com",
                passwordHash: "senha-hash",
                phoneNumber: "11999999999"
            );
        }

        [Fact]
        public void AddEngagementPoints_PontosPositivos_DeveSomarPontuacao()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(20);

            // Assert
            Assert.Equal(20, tutor.ScoreEngagement);
        }

        [Fact]
        public void AddEngagementPoints_PontosZero_DeveLancarArgumentException()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => tutor.AddEngagementPoints(0));

            // Assert
            Assert.Equal(
                "A pontuação deve ser maior que zero.",
                exception.Message);
        }

        [Fact]
        public void AddEngagementPoints_PontosNegativos_DeveLancarArgumentException()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            var exception =
                Assert.Throws<ArgumentException>(
                    () => tutor.AddEngagementPoints(-10));

            // Assert
            Assert.Equal(
                "A pontuação deve ser maior que zero.",
                exception.Message);
        }

        [Fact]
        public void AddEngagementPoints_MenosDe50Pontos_DeveManterAchievementNenhum()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(49);

            // Assert
            Assert.Equal(
                EnumAchievement.Nenhum,
                tutor.Achievement);
        }

        [Fact]
        public void AddEngagementPoints_50Pontos_DeveDefinirInicianteAtencioso()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(50);

            // Assert
            Assert.Equal(
                EnumAchievement.InicianteAtencioso,
                tutor.Achievement);
        }

        [Fact]
        public void AddEngagementPoints_100Pontos_DeveDefinirTutorDedicado()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(100);

            // Assert
            Assert.Equal(
                EnumAchievement.TutorDedicado,
                tutor.Achievement);
        }

        [Fact]
        public void AddEngagementPoints_150Pontos_DeveDefinirGuardiaoPet()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(150);

            // Assert
            Assert.Equal(
                EnumAchievement.GuardiãoPet,
                tutor.Achievement);
        }

        [Fact]
        public void AddEngagementPoints_300Pontos_DeveDefinirClyvoMaster()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(300);

            // Assert
            Assert.Equal(
                EnumAchievement.ClyvoMaster,
                tutor.Achievement);
        }

        [Fact]
        public void AddEngagementPoints_PontuacaoAcumulada_DeveAtualizarAchievement()
        {
            // Arrange
            var tutor = CreateTutor();

            // Act
            tutor.AddEngagementPoints(30);
            tutor.AddEngagementPoints(20);

            // Assert
            Assert.Equal(50, tutor.ScoreEngagement);

            Assert.Equal(
                EnumAchievement.InicianteAtencioso,
                tutor.Achievement);
        }
    }
}
