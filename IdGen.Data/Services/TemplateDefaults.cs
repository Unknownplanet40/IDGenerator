using IdGen.Core.Models.Template;

namespace IdGen.Data.Services;

public static class TemplateDefaults
{
    public static TemplateSideDefinition CreateDefaultFront(int? backgroundAssetId = null)
    {
        var front = new TemplateSideDefinition
        {
            Background = new BackgroundDefinition
            {
                Color = "#F8FAFC",
                AssetId = backgroundAssetId,
                Fit = "Cover"
            }
        };

        // 1. School Logo
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Logo",
            X = 19,
            Y = 10,
            Width = 140,
            Height = 160,
            Locked = true
        });

        // 2. School Title Text (Apex Institute of Science & Technology)
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 143,
            Y = 32,
            Width = 487,
            Height = 67,
            FieldKey = "school_name",
            FontFamily = "Times New Roman",
            FontSize = 48,
            FontWeight = "Bold",
            TextColor = "#FFFFFF",
            TextAlign = "Left",
            Locked = true
        });

        // 3. Campus Name
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 155,
            Y = 108,
            Width = 480,
            Height = 26,
            FieldKey = "campus_name",
            FontFamily = "Arial",
            FontSize = 24,
            FontWeight = "SemiBold",
            TextColor = "#CBD5E1",
            TextAlign = "Left",
            Locked = true
        });

        // 4. Student Photo 2x2
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Photo",
            X = 40,
            Y = 230,
            Width = 280,
            Height = 280,
            Locked = true
        });

        // 5. Student Number
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 44,
            Y = 510,
            Width = 275,
            Height = 54,
            FieldKey = "student_no",
            FontFamily = "Segoe UI",
            FontSize = 28,
            FontWeight = "Bold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 6. Term Rectangle Plate
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 36,
            Y = 584,
            Width = 282,
            Height = 100,
            FillColor = "#1E3A5F",
            FillOpacity = 100,
            CornerRadius = 8
        });

        // 7. Semester
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 36,
            Y = 588,
            Width = 283,
            Height = 35,
            FieldKey = "semester",
            FontFamily = "Segoe UI",
            FontSize = 24,
            FontWeight = "Bold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 8. School Year
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 33,
            Y = 625,
            Width = 286,
            Height = 57,
            FieldKey = "school_year",
            FontFamily = "Segoe UI",
            FontSize = 32,
            FontWeight = "Bold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 9. Full Name
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 12,
            Y = 731,
            Width = 612,
            Height = 71,
            FieldKey = "full_name",
            FontFamily = "Segoe UI",
            FontSize = 32,
            FontWeight = "SemiBold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 10. Course
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 11,
            Y = 805,
            Width = 616,
            Height = 54,
            FieldKey = "course",
            FontFamily = "Segoe UI",
            FontSize = 32,
            FontWeight = "SemiBold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 11. White Card Plate behind Barcode
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 0,
            Y = 920,
            Width = 638,
            Height = 91,
            FillColor = "#FFFFFF",
            BorderColor = "#CBD5E1",
            BorderWidth = 1,
            CornerRadius = 6
        });

        // 12. Full-width Barcode across bottom
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Barcode",
            X = 0,
            Y = 917,
            Width = 638,
            Height = 92,
            BarcodeColor = "#000000",
            ShowHumanReadable = false,
            IncludeQuietZone = false
        });

        return front;
    }

    public static TemplateSideDefinition CreateDefaultBack()
    {
        var back = new TemplateSideDefinition
        {
            Background = new BackgroundDefinition
            {
                Color = "#FFFFFF",
                Fit = "Cover"
            }
        };

        // 1. Signature
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "Signature",
            X = 32,
            Y = 79,
            Width = 581,
            Height = 96
        });

        // 2. Line under signature
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "Line",
            X = 35,
            Y = 179,
            Width = 580,
            Height = 10,
            LineColor = "#000000",
            Thickness = 2
        });

        // 3. Cardholder's signature label
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 35,
            Y = 191,
            Width = 581,
            Height = 22,
            StaticText = "CARDHOLDER'S SIGNATURE",
            FontFamily = "Segoe UI",
            FontSize = 11,
            FontWeight = "Bold",
            TextColor = "#475569",
            TextAlign = "Center"
        });

        // 4. Non-transferable Notice
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 45,
            Y = 234,
            Width = 558,
            Height = 70,
            StaticText = "This card is non-transferable and must be presented upon request. Any alteration or misuse invalidates this card. If found, please return to the Office of Student Affairs.",
            FontFamily = "Segoe UI",
            FontSize = 14,
            FontWeight = "Bold",
            Italic = true,
            TextColor = "#1E293B",
            TextAlign = "Center"
        });

        // 5. Emergency Header Bar
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 31,
            Y = 351,
            Width = 588,
            Height = 40,
            FillColor = "#000000",
            CornerRadius = 0
        });

        // 6. Emergency Title
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 32,
            Y = 356,
            Width = 585,
            Height = 35,
            StaticText = "IN CASE OF EMERGENCY, PLS NOTIFY:",
            FontFamily = "Segoe UI",
            FontSize = 18,
            FontWeight = "SemiBold",
            TextColor = "#FFFFFF",
            TextAlign = "Center"
        });

        // 7. Emergency Box
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 31,
            Y = 385,
            Width = 590,
            Height = 215,
            FillColor = "#FFFFFF",
            BorderColor = "#000000",
            BorderWidth = 2,
            CornerRadius = 0
        });

        // 8. Guardian Name label & dynamic field
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 48,
            Y = 398,
            Width = 140,
            Height = 35,
            StaticText = "NAME:",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "Bold",
            TextColor = "#0F172A"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 190,
            Y = 398,
            Width = 420,
            Height = 35,
            FieldKey = "guardian_name",
            FontFamily = "Segoe UI",
            FontSize = 20,
            TextColor = "#0F172A",
            TextAlign = "Left"
        });

        // 9. Address label & dynamic field
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 49,
            Y = 435,
            Width = 136,
            Height = 35,
            StaticText = "ADDRESS:",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "Bold",
            TextColor = "#0F172A"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 190,
            Y = 435,
            Width = 420,
            Height = 70,
            FieldKey = "address",
            FontFamily = "Segoe UI",
            FontSize = 18,
            TextColor = "#0F172A",
            TextAlign = "Left"
        });

        // 10. Contact label & dynamic field
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 47,
            Y = 552,
            Width = 174,
            Height = 35,
            StaticText = "CONTACT NO:",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "Bold",
            TextColor = "#0F172A"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 220,
            Y = 552,
            Width = 390,
            Height = 35,
            FieldKey = "guardian_phone",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "SemiBold",
            TextColor = "#0F172A",
            TextAlign = "Left"
        });

        // 11. Gender
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 34,
            Y = 608,
            Width = 100,
            Height = 35,
            StaticText = "Gender:",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "SemiBold",
            TextColor = "#0F172A"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 135,
            Y = 608,
            Width = 150,
            Height = 35,
            FieldKey = "gender",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "Bold",
            TextColor = "#0F172A"
        });

        // 12. Blood Type
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 292,
            Y = 608,
            Width = 140,
            Height = 35,
            StaticText = "Blood Type:",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "SemiBold",
            TextColor = "#0F172A"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 435,
            Y = 608,
            Width = 180,
            Height = 35,
            FieldKey = "blood_type",
            FontFamily = "Segoe UI",
            FontSize = 20,
            FontWeight = "Bold",
            TextColor = "#0F172A"
        });

        // 13. Registrar Signatory Line & Text
        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "Line",
            X = 149,
            Y = 860,
            Width = 340,
            Height = 1,
            LineColor = "#000000",
            Thickness = 1
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 40,
            Y = 870,
            Width = 558,
            Height = 28,
            FieldKey = "principal_name",
            FontFamily = "Segoe UI",
            FontSize = 16,
            FontWeight = "Bold",
            TextColor = "#0F172A",
            TextAlign = "Center"
        });

        back.Elements.Add(new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 40,
            Y = 902,
            Width = 558,
            Height = 24,
            FieldKey = "principal_title",
            FontFamily = "Segoe UI",
            FontSize = 13,
            TextColor = "#64748B",
            TextAlign = "Center"
        });

        return back;
    }
}
