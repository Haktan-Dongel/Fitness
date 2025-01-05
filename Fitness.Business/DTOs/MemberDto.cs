using System;

namespace Fitness.Business.DTOs
{
    public record MemberDto(
        int MemberId,
        string FirstName,
        string LastName,
        string Email,
        string Address,
        DateTime Birthday,
        string? Interests,
        string MemberType
    );

    public record CreateMemberDto(
        string FirstName,
        string LastName,
        string Email,
        string Address,
        DateTime Birthday,
        string? Interests
    );

    public record UpdateMemberDto(
        int MemberId,
        string FirstName,
        string LastName,
        string Email,
        string Address
    );
}
