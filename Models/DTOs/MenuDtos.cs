namespace RestAPISystemKnowledge.Models.DTOs
{
    public class CreateMenuRequestDto
    {
        public string Label { get; set; } = string.Empty;
        public string RouteUrl { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? Placement { get; set; }
        public int OrderIndex { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
