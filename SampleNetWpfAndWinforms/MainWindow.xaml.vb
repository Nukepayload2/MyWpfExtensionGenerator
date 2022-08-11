Class MainWindow
    Private Sub MainWindow_Loaded() Handles Me.Loaded
        My.Windows.Window1.Show
        My.Computer.Audio.PlaySystemSound(System.Media.SystemSounds.Beep)
    End Sub
End Class
