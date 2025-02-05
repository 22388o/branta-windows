﻿using Branta.Classes;
using Branta.Enums;
using System.Windows;
using System.Windows.Input;

namespace Branta.Views;

public partial class HelpWindow : BaseWindow
{
    public HelpWindow()
    {
        InitializeComponent();

        SetLanguageDictionary();

        TbLink.Foreground = Color.Brush(Color.Gold);

        TbVersion.Text = "v" + Helper.GetBrantaVersionWithoutCommitHash();
    }

    private void Link_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            Helper.OpenLink("https://branta.pro");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening webpage: {ex.Message}");
        }
    }
}
