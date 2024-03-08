// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Users.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public string? Username { get; set; }
    public bool Enabled { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public Dictionary<string, string[]>? Attributes { get; set; }
}
