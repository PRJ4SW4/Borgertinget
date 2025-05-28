using backend.DTO.LearningEnvironment;
using backend.Models.LearningEnvironment;
using backend.Repositories.LearningEnvironment;
using backend.Services.LearningEnvironment;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Services
{
    [TestFixture]
    public class PagesServiceTest
    {
        private LearningPageService _uut;
        private ILearningPageRepository _repository;
        private ILogger<LearningPageService> _logger;

        [SetUp]
        public void SetUp()
        {
            _repository = Substitute.For<ILearningPageRepository>();
            _logger = Substitute.For<ILogger<LearningPageService>>();
            _uut = new LearningPageService(_repository, _logger);
        }

        [Test]
        public async Task CreatePageAsync_ShouldReturnCreatedPage()
        {
            // Arrange
            var createRequest = new PageCreateRequestDTO
            {
                Title = "Lorem Ipsum Title",
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                ParentPageId = null,
            };
            var page = new Page
            {
                Id = 1,
                Title = createRequest.Title,
                Content = createRequest.Content,
                ParentPageId = createRequest.ParentPageId,
                DisplayOrder = 1,
                AssociatedQuestions = new List<Question>
                {
                    new Question
                    {
                        QuestionId = 10,
                        QuestionText = "What is Lorem Ipsum?",
                        PageId = 1,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption
                            {
                                AnswerOptionId = 100,
                                OptionText = "A placeholder text",
                                IsCorrect = true,
                                DisplayOrder = 1,
                                QuestionId = 10,
                            },
                            new AnswerOption
                            {
                                AnswerOptionId = 101,
                                OptionText = "A real language",
                                IsCorrect = false,
                                DisplayOrder = 2,
                                QuestionId = 10,
                            },
                        },
                    },
                },
                ChildPages = new List<Page>
                {
                    new Page
                    {
                        Id = 2,
                        Title = "Child Page",
                        Content = "Child content.",
                        DisplayOrder = 2,
                    },
                },
            };
            var allPages = new List<Page> { page };

            // Simulate DB-generated ID assignment
            _repository
                .When(x => x.AddPageAsync(Arg.Any<Page>()))
                .Do(call => call.Arg<Page>().Id = 1);

            _repository.SaveChangesAsync().Returns(1);
            _repository.GetPageWithDetailsAsync(Arg.Any<int>()).ReturnsForAnyArgs(page);
            _repository.GetAllPagesAsync().Returns(allPages);
            _repository.GetPageByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(page);
            _repository.GetChildPagesOrderedAsync(Arg.Any<int?>()).Returns(allPages);

            // Act
            var result = await _uut.CreatePageAsync(createRequest);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Title, Is.EqualTo(createRequest.Title));
            Assert.That(result.Content, Is.EqualTo(createRequest.Content));
            Assert.That(page.AssociatedQuestions, Is.Not.Empty);
            var firstQuestion = page.AssociatedQuestions.First();
            Assert.That(firstQuestion.QuestionText, Is.EqualTo("What is Lorem Ipsum?"));
            Assert.That(firstQuestion.AnswerOptions, Is.Not.Empty);
            var firstOption = firstQuestion.AnswerOptions.First();
            Assert.That(firstOption.OptionText, Is.EqualTo("A placeholder text"));
            Assert.That(page.ChildPages, Is.Not.Empty);
        }

        [Test]
        public async Task UpdatePageAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateRequest = new PageUpdateRequestDTO
            {
                Id = 1,
                Title = "Updated Lorem Ipsum Title",
                Content = "Updated lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                ParentPageId = null,
                DisplayOrder = 1,
                AssociatedQuestions = new List<QuestionCreateOrUpdateDTO>
                {
                    new QuestionCreateOrUpdateDTO
                    {
                        Id = 10,
                        QuestionText = "What is Lorem Ipsum? (updated)",
                        Options = new List<AnswerOptionCreateOrUpdateDTO>
                        {
                            new AnswerOptionCreateOrUpdateDTO
                            {
                                Id = 100,
                                OptionText = "A placeholder text (updated)",
                                IsCorrect = true,
                                DisplayOrder = 1,
                            },
                            new AnswerOptionCreateOrUpdateDTO
                            {
                                Id = 101,
                                OptionText = "A real language (updated)",
                                IsCorrect = false,
                                DisplayOrder = 2,
                            },
                        },
                    },
                },
            };
            var page = new Page
            {
                Id = 1,
                Title = "Old Title",
                Content = "Old content.",
                ParentPageId = null,
                DisplayOrder = 1,
                AssociatedQuestions = new List<Question>
                {
                    new Question
                    {
                        QuestionId = 10,
                        QuestionText = "What is Lorem Ipsum?",
                        PageId = 1,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption
                            {
                                AnswerOptionId = 100,
                                OptionText = "A placeholder text",
                                IsCorrect = true,
                                DisplayOrder = 1,
                                QuestionId = 10,
                            },
                            new AnswerOption
                            {
                                AnswerOptionId = 101,
                                OptionText = "A real language",
                                IsCorrect = false,
                                DisplayOrder = 2,
                                QuestionId = 10,
                            },
                        },
                    },
                },
                ChildPages = new List<Page>
                {
                    new Page
                    {
                        Id = 2,
                        Title = "Child Page",
                        Content = "Child content.",
                        DisplayOrder = 2,
                    },
                },
            };
            var allPages = new List<Page> { page };
            _repository.GetPageByIdAsync(Arg.Any<int>()).ReturnsForAnyArgs(page);
            _repository.SaveChangesAsync().Returns(1);
            _repository.GetAllPagesAsync().Returns(allPages);
            _repository.GetChildPagesOrderedAsync(Arg.Any<int?>()).Returns(allPages);
            _repository.GetPageWithDetailsAsync(Arg.Any<int>()).ReturnsForAnyArgs(page);
            _repository.When(x => x.UpdatePage(Arg.Any<Page>())).Do(_ => { }); // Mock UpdatePage

            // Act
            var result = await _uut.UpdatePageAsync(1, updateRequest);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(page.Title, Is.EqualTo(updateRequest.Title));
            Assert.That(page.Content, Is.EqualTo(updateRequest.Content));
            Assert.That(page.AssociatedQuestions, Is.Not.Empty);
            var updatedQuestion = page.AssociatedQuestions.First();
            Assert.That(updatedQuestion.QuestionText, Is.EqualTo("What is Lorem Ipsum? (updated)"));
            Assert.That(updatedQuestion.AnswerOptions, Is.Not.Empty);
            var updatedOption = updatedQuestion.AnswerOptions.First();
            Assert.That(updatedOption.OptionText, Is.EqualTo("A placeholder text (updated)"));
        }

        [Test]
        public async Task UpdatePageAsync_ShouldReturnFalse_WhenPageDoesNotExist()
        {
            // Arrange
            var updateRequest = new PageUpdateRequestDTO { Id = 1, Title = "Updated Page" };
            _repository.GetPageByIdAsync(1).Returns((Page?)null);

            // Act
            var result = await _uut.UpdatePageAsync(1, updateRequest);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeletePageAsync_ShouldReturnTrue_WhenDeletionIsSuccessful()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "Lorem Ipsum Title",
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                AssociatedQuestions = new List<Question>
                {
                    new Question
                    {
                        QuestionId = 10,
                        QuestionText = "What is Lorem Ipsum?",
                        PageId = 1,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new AnswerOption
                            {
                                AnswerOptionId = 100,
                                OptionText = "A placeholder text",
                                IsCorrect = true,
                                DisplayOrder = 1,
                                QuestionId = 10,
                            },
                            new AnswerOption
                            {
                                AnswerOptionId = 101,
                                OptionText = "A real language",
                                IsCorrect = false,
                                DisplayOrder = 2,
                                QuestionId = 10,
                            },
                        },
                    },
                },
            };
            _repository.GetPageWithDetailsAsync(1).Returns(page);
            _repository.SaveChangesAsync().Returns(1);
            _repository.When(x => x.RemoveRangeAnswerOptions(Arg.Any<IEnumerable<AnswerOption>>()));
            _repository.When(x => x.RemoveRangeQuestions(Arg.Any<IEnumerable<Question>>()));
            _repository.When(x => x.RemovePage(Arg.Any<Page>()));

            // Act
            var result = await _uut.DeletePageAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeletePageAsync_ShouldReturnFalse_WhenPageDoesNotExist()
        {
            // Arrange
            _repository.GetPageByIdAsync(1).Returns((Page?)null);

            // Act
            var result = await _uut.DeletePageAsync(1);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
