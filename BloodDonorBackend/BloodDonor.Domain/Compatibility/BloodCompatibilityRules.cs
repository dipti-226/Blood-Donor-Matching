using BloodDonor.Domain.Entities;

namespace BloodDonor.Domain.Compatibility
{
    /// <summary>
    /// Deterministic ABO / Rh compatibility rules, keyed by RECIPIENT blood group.
    /// Each entry lists the DONOR blood groups acceptable for that recipient.
    ///
    /// SAFETY NOTE: these are standard textbook rules. They must be reviewed by a qualified
    /// medical / blood-bank professional before any production use. They only describe
    /// compatibility. They never decide donor eligibility, medical suitability, or verification.
    /// Rule changes must go through code review and a deployment, never a runtime edit.
    /// </summary>
    public static class BloodCompatibilityRules
    {
        // Red blood cells: the donor's red cells must not carry an antigen the recipient lacks.
        private static readonly IReadOnlyDictionary<BloodGroup, BloodGroup[]> RedBloodCellDonors =
            new Dictionary<BloodGroup, BloodGroup[]>
            {
                [BloodGroup.OMinus] = new[] { BloodGroup.OMinus },
                [BloodGroup.OPlus] = new[] { BloodGroup.OPlus, BloodGroup.OMinus },
                [BloodGroup.AMinus] = new[] { BloodGroup.AMinus, BloodGroup.OMinus },
                [BloodGroup.APlus] = new[] { BloodGroup.APlus, BloodGroup.AMinus, BloodGroup.OPlus, BloodGroup.OMinus },
                [BloodGroup.BMinus] = new[] { BloodGroup.BMinus, BloodGroup.OMinus },
                [BloodGroup.BPlus] = new[] { BloodGroup.BPlus, BloodGroup.BMinus, BloodGroup.OPlus, BloodGroup.OMinus },
                [BloodGroup.ABMinus] = new[] { BloodGroup.ABMinus, BloodGroup.AMinus, BloodGroup.BMinus, BloodGroup.OMinus },
                [BloodGroup.ABPlus] = new[]
                {
                    BloodGroup.APlus, BloodGroup.AMinus,
                    BloodGroup.BPlus, BloodGroup.BMinus,
                    BloodGroup.ABPlus, BloodGroup.ABMinus,
                    BloodGroup.OPlus, BloodGroup.OMinus
                }
            };

        // Plasma: ABO only. Rh is not carried on plasma, so either Rh sign is acceptable.
        // The donor's plasma must not carry an antibody against the recipient's red-cell antigens.
        private static readonly IReadOnlyDictionary<BloodGroup, BloodGroup[]> PlasmaDonors =
            BuildPlasmaDonors();

        // Whole blood and platelets carry both red cells and plasma, so the conservative rule is
        // the intersection: the donor must satisfy BOTH the red-cell rule and the plasma rule.
        private static readonly IReadOnlyDictionary<BloodGroup, BloodGroup[]> WholeBloodAndPlateletDonors =
            BuildIntersection();

        public static IReadOnlyList<BloodGroup> GetCompatibleDonorGroups(BloodGroup recipient, BloodComponent component)
        {
            if (!Enum.IsDefined(recipient))
            {
                throw new ArgumentOutOfRangeException(nameof(recipient), "Unknown blood group.");
            }

            return component switch
            {
                BloodComponent.RedBloodCells => RedBloodCellDonors[recipient],
                BloodComponent.Plasma => PlasmaDonors[recipient],
                BloodComponent.WholeBlood => WholeBloodAndPlateletDonors[recipient],
                BloodComponent.Platelets => WholeBloodAndPlateletDonors[recipient],
                _ => throw new ArgumentOutOfRangeException(nameof(component), "Unknown blood component.")
            };
        }

        private static IReadOnlyDictionary<BloodGroup, BloodGroup[]> BuildPlasmaDonors()
        {
            // Acceptable donor ABO types per recipient ABO type.
            var aboRules = new Dictionary<string, string[]>
            {
                ["O"] = new[] { "O", "A", "B", "AB" },
                ["A"] = new[] { "A", "AB" },
                ["B"] = new[] { "B", "AB" },
                ["AB"] = new[] { "AB" }
            };

            var result = new Dictionary<BloodGroup, BloodGroup[]>();

            foreach (var recipient in Enum.GetValues<BloodGroup>())
            {
                var acceptedAbo = aboRules[AboOf(recipient)];

                result[recipient] = Enum.GetValues<BloodGroup>()
                    .Where(donor => acceptedAbo.Contains(AboOf(donor)))
                    .OrderBy(donor => donor)
                    .ToArray();
            }

            return result;
        }

        private static IReadOnlyDictionary<BloodGroup, BloodGroup[]> BuildIntersection()
        {
            var result = new Dictionary<BloodGroup, BloodGroup[]>();

            foreach (var recipient in Enum.GetValues<BloodGroup>())
            {
                result[recipient] = RedBloodCellDonors[recipient]
                    .Intersect(PlasmaDonors[recipient])
                    .OrderBy(donor => donor)
                    .ToArray();
            }

            return result;
        }

        private static string AboOf(BloodGroup group)
        {
            return group switch
            {
                BloodGroup.APlus or BloodGroup.AMinus => "A",
                BloodGroup.BPlus or BloodGroup.BMinus => "B",
                BloodGroup.ABPlus or BloodGroup.ABMinus => "AB",
                BloodGroup.OPlus or BloodGroup.OMinus => "O",
                _ => throw new ArgumentOutOfRangeException(nameof(group), "Unknown blood group.")
            };
        }
    }
}