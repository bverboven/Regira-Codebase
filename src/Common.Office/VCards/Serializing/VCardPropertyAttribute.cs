namespace Regira.Office.VCards.Serializing;

public class VCardPropertyAttribute : Attribute
{
    public VCardPropertyAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}