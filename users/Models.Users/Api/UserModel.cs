// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Users.Dto;

namespace Econolite.Ode.Models.Users.Api
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public long CreatedTimestamp { get; set; }

        public string Username { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        public Dictionary<string, string[]> Attributes { get; set; } = new Dictionary<string, string[]>();

        // More available if needed, didn't bother listing them since didn't seem useful

        public UserDto ToDto()
        {
            return new UserDto
            {
                Id = Id,
                CreatedTimestamp = new DateTime(CreatedTimestamp),
                Username = Username,
                Enabled = Enabled,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                EmailVerified = EmailVerified,
                Attributes = Attributes,
            };
        }
    }
}
