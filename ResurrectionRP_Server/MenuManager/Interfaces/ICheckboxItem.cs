namespace ResurrectionRP_Server
{
    interface ICheckboxItem
    {
        bool Checked { get; set; }

        bool IsInput();
    }
}