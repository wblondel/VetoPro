using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Contracts.DTOs.Management;

namespace VetoPro.Api.Mapping;

public static class ContactMapper
{
    /// <summary>
    /// Maps a Contact entity to a ContactDto.
    /// Assumes related entities (User, StaffDetails) might be loaded.
    /// </summary>
    public static ContactDto ToDto(this Contact contact)
    {
        StaffDetailsDto? staffDetailsDto = null;
        if (contact.StaffDetails != null)
        {
            staffDetailsDto = new StaffDetailsDto
            {
                Id = contact.StaffDetails.Id,
                Role = contact.StaffDetails.Role,
                LicenseNumber = contact.StaffDetails.LicenseNumber,
                Specialty = contact.StaffDetails.Specialty,
                IsActive = contact.StaffDetails.IsActive
            };
        }

        return new ContactDto
        {
            Id = contact.Id,
            LoginEmail = contact.User?.Email, // User might be null
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email, // Email from Contact profile
            PhoneNumber = contact.PhoneNumber,
            AddressLine1 = contact.AddressLine1,
            City = contact.City,
            PostalCode = contact.PostalCode,
            Country = contact.Country,
            IsOwner = contact.IsOwner,
            IsClient = contact.IsClient,
            IsStaff = contact.IsStaff,
            StaffDetails = staffDetailsDto // Mapped StaffDetails or null
        };
    }
}