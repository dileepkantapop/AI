using Experimental.Skills.Tests.Utterances;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Experimental.Skills.Tests
{
    [TestClass]
    public class ExperimentalSkillTests : SkillTestBase
    {
        private Dictionary<string, SkillDefinition> skillDefinitions;

        [TestInitialize]
        public void InitSkills()
        {
            this.skillDefinitions = new Dictionary<string, SkillDefinition>();
            this.skillDefinitions.Add("newsSkill", this.CreateSkillDefinition("newsSkill", typeof(NewsSkill.NewsSkill)));
            this.skillDefinitions.Add("restaurantSkill", this.CreateSkillDefinition("restaurantSkill", typeof(RestaurantBooking.RestaurantBooking)));

            foreach (SkillDefinition skill in this.skillDefinitions.Values)
            {
                // Add the SkillDialog to the available dialogs passing the initialized FakeSkill
                this.Dialogs.Add(new SkillDialog(skill, this.Services, this.ProactiveState, this.EndpointService, this.TelemetryClient, this.BackgroundTaskQueue));
            }
        }

        [TestMethod]
        public async Task NewsSkillInvocation()
        {
            this.SkillDialogOptions = new SkillDialogOptions();
            this.SkillDialogOptions.SkillDefinition = this.skillDefinitions["newsSkill"];

            await this.GetTestFlow()
            .Send(ExperimentalUtterances.FindNews)
            .AssertReply("What topic are you interested in?")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task RestaurantSkillInvocation()
        {
            this.SkillDialogOptions = new SkillDialogOptions();
            this.SkillDialogOptions.SkillDefinition = this.skillDefinitions["restaurantSkill"];

            await this.GetTestFlow()
            .Send(ExperimentalUtterances.BookRestaurant)
            .AssertReply(ValidateAzureMapsKeyPrompt())
            .StartTestAsync();
        }

        private Action<IActivity> ValidateAzureMapsKeyPrompt()
        {
            return activity =>
            {
                var traceActivity = activity as Activity;
                Assert.IsNotNull(traceActivity);

                Assert.IsTrue(traceActivity.Text.Contains("DialogException: Could not get the required Azure Maps key. Please make sure your settings are correctly configured."));
            };
        }
    }
}