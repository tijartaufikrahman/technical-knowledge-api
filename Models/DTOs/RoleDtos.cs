namespace RestAPISystemKnowledge.Models.DTOs
{
    public class RoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string NamaRole { get; set; } = string.Empty;
        public string Deskripsi { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateRoleDto
    {
        public string NamaRole { get; set; } = string.Empty;
        public string? Deskripsi { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EditRoleDto
    {
        public long Id { get; set; }
        public string NamaRole { get; set; } = string.Empty;
        public string? Deskripsi { get; set; }
        public bool? IsActive { get; set; }
    }
}
