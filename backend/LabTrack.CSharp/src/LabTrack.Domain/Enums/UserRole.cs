namespace LabTrack.Domain.Enums;

public enum UserRole
{
    Student = 0,
    Instructor = 1,
    Technician = 2,
    Admin = 2,
    AdminTechnician = 2,

    STUDENT = Student,
    INSTRUCTOR = Instructor,
    TECHNICIAN = Technician,
    ADMIN = Technician,
    ADMIN_TECHNICIAN = Technician
}
