namespace SilkenImpact {
    interface IVisibilityController {
        public void Inspect(HealthManager healthManager);
        public bool Update(bool forceCheck = false);
        public bool IsVisible { get; set; }

    }
}