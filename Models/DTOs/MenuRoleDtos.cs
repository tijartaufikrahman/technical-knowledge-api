namespace RestAPISystemKnowledge.Models.DTOs
{
    public class MenuRoleDto
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public string NamaMenu { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Urutan { get; set; }
    }

    public class CreateMenuRoleDto
    {
        public long RoleId { get; set; }
        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public string? NamaMenu { get; set; }
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int Urutan { get; set; }
    }

    public class EditMenuRoleDto
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public long? MenuId { get; set; }
        public long? GroupId { get; set; }
        public string? NamaMenu { get; set; }
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int Urutan { get; set; }
    }
}
