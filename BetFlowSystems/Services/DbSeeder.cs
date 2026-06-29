using BetFlowSystems.Models.DbModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Admin>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Clerk", "Manager", "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@betflowsystems.com";
            string adminPassword = "Admin@BFS-2026!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new Admin
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        public static async Task SeedBetTypesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (await context.BetTypes.AnyAsync())
                return;


            var betTypes = new List<BetType>
            {
                new() { Sport = "Football", EventName = "Match Winner", Description = "Predict winner of match" },
                new() { Sport = "Football", EventName = "Total Goals Over/Under", Description = "Predict total goals" },
                new() { Sport = "Football", EventName = "First Goal Scorer", Description = "Player to score first" },
                new() { Sport = "Football", EventName = "Both Teams Score", Description = "Will both teams score" },
                new() { Sport = "Football", EventName = "Half Time Result", Description = "Result at half time" },

                new() { Sport = "Basketball", EventName = "Match Winner", Description = "Predict winner" },
                new() { Sport = "Basketball", EventName = "Total Points", Description = "Total points scored" },
                new() { Sport = "Basketball", EventName = "First Quarter Winner", Description = "Winner of Q1" },
                new() { Sport = "Basketball", EventName = "Handicap Betting", Description = "Point spread" },
                new() { Sport = "Basketball", EventName = "Player Points Over", Description = "Player scoring" },

                new() { Sport = "Tennis", EventName = "Match Winner", Description = "Winner of match" },
                new() { Sport = "Tennis", EventName = "Set Betting", Description = "Correct set score" },
                new() { Sport = "Tennis", EventName = "Total Games", Description = "Total games played" },
                new() { Sport = "Tennis", EventName = "First Set Winner", Description = "Winner first set" },
                new() { Sport = "Tennis", EventName = "Tie Break Occurrence", Description = "Will there be a tie break" },

                new() { Sport = "Cricket", EventName = "Match Winner", Description = "Winner of match" },
                new() { Sport = "Cricket", EventName = "Top Batsman", Description = "Highest run scorer" },
                new() { Sport = "Cricket", EventName = "Top Bowler", Description = "Most wickets" },
                new() { Sport = "Cricket", EventName = "Total Runs", Description = "Total match runs" },
                new() { Sport = "Cricket", EventName = "Over Runs", Description = "Runs in specific over" },

                new() { Sport = "Rugby", EventName = "Match Winner", Description = "Winner prediction" },
                new() { Sport = "Rugby", EventName = "Total Points", Description = "Points scored" },
                new() { Sport = "Rugby", EventName = "First Try Scorer", Description = "First try" },
                new() { Sport = "Rugby", EventName = "Handicap", Description = "Point spread" },
                new() { Sport = "Rugby", EventName = "Half Time Result", Description = "Half result" },

                new() { Sport = "Baseball", EventName = "Match Winner", Description = "Winner prediction" },
                new() { Sport = "Baseball", EventName = "Total Runs", Description = "Total runs" },
                new() { Sport = "Baseball", EventName = "First Inning Runs", Description = "Runs inning 1" },
                new() { Sport = "Baseball", EventName = "Home Runs", Description = "Total home runs" },
                new() { Sport = "Baseball", EventName = "Pitcher Strikeouts", Description = "Strikeout bets" },

                new() { Sport = "Hockey", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Hockey", EventName = "Total Goals", Description = "Total goals" },
                new() { Sport = "Hockey", EventName = "First Goal Scorer", Description = "First goal" },
                new() { Sport = "Hockey", EventName = "Period Winner", Description = "Period bet" },
                new() { Sport = "Hockey", EventName = "Penalty Minutes", Description = "Penalty bets" },

                new() { Sport = "Golf", EventName = "Tournament Winner", Description = "Winner" },
                new() { Sport = "Golf", EventName = "Top 5 Finish", Description = "Top 5" },
                new() { Sport = "Golf", EventName = "Top 10 Finish", Description = "Top 10" },
                new() { Sport = "Golf", EventName = "Round Leader", Description = "Leader round" },
                new() { Sport = "Golf", EventName = "Hole In One", Description = "Special prop" },

                new() { Sport = "MMA", EventName = "Fight Winner", Description = "Winner" },
                new() { Sport = "MMA", EventName = "Method Of Victory", Description = "KO/Sub/Decision" },
                new() { Sport = "MMA", EventName = "Round Betting", Description = "Round outcome" },
                new() { Sport = "MMA", EventName = "Fight Duration", Description = "Length of fight" },
                new() { Sport = "MMA", EventName = "Knockdowns", Description = "Knockdowns" },

                new() { Sport = "Boxing", EventName = "Fight Winner", Description = "Winner" },
                new() { Sport = "Boxing", EventName = "Round Betting", Description = "Round outcome" },
                new() { Sport = "Boxing", EventName = "Method Of Victory", Description = "KO/Decision" },
                new() { Sport = "Boxing", EventName = "Total Rounds", Description = "Rounds total" },
                new() { Sport = "Boxing", EventName = "Knockdown Occurrence", Description = "Knockdown" },

                new() { Sport = "Esports", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Esports", EventName = "Total Maps", Description = "Map count" },
                new() { Sport = "Esports", EventName = "First Blood", Description = "First kill" },
                new() { Sport = "Esports", EventName = "Map Winner", Description = "Map bet" },
                new() { Sport = "Esports", EventName = "Kills Over", Description = "Kill count" },

                new() { Sport = "Horse Racing", EventName = "Race Winner", Description = "Winner" },
                new() { Sport = "Horse Racing", EventName = "Place Bet", Description = "Top placement" },
                new() { Sport = "Horse Racing", EventName = "Exacta", Description = "Top 2 order" },
                new() { Sport = "Horse Racing", EventName = "Trifecta", Description = "Top 3 order" },
                new() { Sport = "Horse Racing", EventName = "Each Way", Description = "Win/place" },

                new() { Sport = "Cycling", EventName = "Stage Winner", Description = "Stage winner" },
                new() { Sport = "Cycling", EventName = "Overall Winner", Description = "Tour winner" },
                new() { Sport = "Cycling", EventName = "King of Mountain", Description = "Climb leader" },
                new() { Sport = "Cycling", EventName = "Sprint Winner", Description = "Sprint" },
                new() { Sport = "Cycling", EventName = "Top 3 Finish", Description = "Top 3" },

                new() { Sport = "Volleyball", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Volleyball", EventName = "Set Betting", Description = "Set score" },
                new() { Sport = "Volleyball", EventName = "Total Points", Description = "Points total" },
                new() { Sport = "Volleyball", EventName = "First Set Winner", Description = "Set 1" },
                new() { Sport = "Volleyball", EventName = "Handicap", Description = "Spread" },

                new() { Sport = "Badminton", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Badminton", EventName = "Set Betting", Description = "Set scores" },
                new() { Sport = "Badminton", EventName = "Total Points", Description = "Points" },
                new() { Sport = "Badminton", EventName = "First Game Winner", Description = "Game 1" },
                new() { Sport = "Badminton", EventName = "Handicap", Description = "Spread" },

                new() { Sport = "Table Tennis", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Table Tennis", EventName = "Set Betting", Description = "Set scores" },
                new() { Sport = "Table Tennis", EventName = "Total Points", Description = "Points total" },
                new() { Sport = "Table Tennis", EventName = "First Set", Description = "First set" },
                new() { Sport = "Table Tennis", EventName = "Handicap", Description = "Spread" },

                new() { Sport = "Snooker", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Snooker", EventName = "Frame Betting", Description = "Frames" },
                new() { Sport = "Snooker", EventName = "Total Frames", Description = "Frame total" },
                new() { Sport = "Snooker", EventName = "Century Break", Description = "100+ break" },
                new() { Sport = "Snooker", EventName = "Highest Break", Description = "Top break" },

                new() { Sport = "Darts", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "Darts", EventName = "Total Legs", Description = "Legs total" },
                new() { Sport = "Darts", EventName = "180s Count", Description = "180 hits" },
                new() { Sport = "Darts", EventName = "Set Betting", Description = "Set outcome" },
                new() { Sport = "Darts", EventName = "Highest Checkout", Description = "Checkout" },

                new() { Sport = "American Football", EventName = "Match Winner", Description = "Winner" },
                new() { Sport = "American Football", EventName = "Total Points", Description = "Points total" },
                new() { Sport = "American Football", EventName = "First Touchdown", Description = "TD scorer" },
                new() { Sport = "American Football", EventName = "Spread Betting", Description = "Spread" },
                new() { Sport = "American Football", EventName = "Quarter Result", Description = "Quarter" },

                new() { Sport = "Formula 1", EventName = "Race Winner", Description = "Winner" },
                new() { Sport = "Formula 1", EventName = "Pole Position", Description = "Pole sitter" },
                new() { Sport = "Formula 1", EventName = "Fastest Lap", Description = "Fastest lap" },
                new() { Sport = "Formula 1", EventName = "Podium Finish", Description = "Top 3" },
                new() { Sport = "Formula 1", EventName = "Safety Car", Description = "Safety car occurrence" }
            };


            await context.BetTypes.AddRangeAsync(betTypes);
            await context.SaveChangesAsync();
        }
    }
}