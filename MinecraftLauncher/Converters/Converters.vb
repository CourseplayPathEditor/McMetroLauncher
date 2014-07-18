﻿Imports System.IO
Imports Craft.Net
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

#Region "Converters"
Public Structure ImageConvert
    ''' <summary>
    ''' Konvertiert ein Bild in einen Base64-String
    ''' </summary>
    ''' <param name="image">
    ''' Zu konvertierendes Bild
    ''' </param>
    ''' <returns>
    ''' Base64 Repräsentation des Bildes
    ''' </returns>
    Public Shared Function GetStringFromImage(image As System.Drawing.Image) As String
        If image IsNot Nothing Then
            Dim ic As New ImageConverter()
            Dim buffer As Byte() = DirectCast(ic.ConvertTo(image, GetType(Byte())), Byte())
            Return Convert.ToBase64String(buffer, Base64FormattingOptions.InsertLineBreaks)
        Else
            Return Nothing
        End If
    End Function
    '---------------------------------------------------------------------
    ''' <summary>
    ''' Konvertiert einen Base64-String zu einem Bild
    ''' </summary>
    ''' <param name="base64String">
    ''' Zu konvertierender String
    ''' </param>
    ''' <returns>
    ''' Bild das aus dem String erzeugt wird
    ''' </returns>
    Public Shared Function GetImageFromString(base64String As String) As System.Drawing.Image
        Dim buffer As Byte() = Convert.FromBase64String(base64String)

        If buffer IsNot Nothing Then
            Dim ic As New ImageConverter()
            Return TryCast(ic.ConvertFrom(buffer), System.Drawing.Image)
        Else
            Return Nothing
        End If
    End Function

    Public Shared Function GetImageStream(Image As System.Drawing.Image) As BitmapSource
        Dim ms As New MemoryStream()
        Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
        ms.Position = 0
        Dim bi As New BitmapImage()
        bi.BeginInit()
        bi.StreamSource = ms
        bi.EndInit()

        Return bi
    End Function

End Structure

Public Class Base64ImageConverter
    Implements System.Windows.Data.IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim s As String = TryCast(value, String)

        If s Is Nothing Then Return Nothing

        Dim bi As New BitmapImage()

        bi.BeginInit()
        bi.StreamSource = New MemoryStream(System.Convert.FromBase64String(s))
        bi.EndInit()

        Return bi
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class
Public Class FormattingcodesDocumentConverter
    Implements System.Windows.Data.IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim s As String = TryCast(value, String)
        If s Is Nothing Then Return Nothing
        Return FormattingCodes.MinecraftText2Document(s)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class

''' <summary>
''' Convertiert eine IList(Of ServerStatus.Playerlist.Player) zu einem String, getrennt durch neue Zeilen
''' </summary>
''' <remarks></remarks>
Public Class Playerlist_Namesstring_Converter
    Implements System.Windows.Data.IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim s As IList(Of ServerStatus.PlayerList.Player) = TryCast(value, IList(Of ServerStatus.PlayerList.Player))
        If s Is Nothing Then Return Nothing
        Dim playernames As IList(Of String) = s.Select(Function(p) p.Name).ToList
        Dim returnstring As String = "Players:" & Environment.NewLine & String.Join(Environment.NewLine, playernames)
        Return returnstring
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class
Public Class MODS_installed_imageConverter
    Implements System.Windows.Data.IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim s As IList(Of Modifications.Mod.Version) = TryCast(value, IList(Of Modifications.Mod.Version))
        If s Is Nothing Then Return Nothing
        Dim r As BitmapSource
        If s.Where(Function(p) p.version = SelectedModVersion).First.installed = True Then
            r = ImageConvert.GetImageStream(My.Resources.check_green)
        Else
            r = Nothing
        End If
        Return r
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class

Public Class Modified_Date_Converter
    Implements System.Windows.Data.IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim unixTime As Long = CLng(value)
        If unixTime = Nothing Then Return Nothing
        Dim epoch As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Return epoch.AddSeconds(unixTime).ToString("G")
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class

Public Class userid_OnlineModeConverter
    Implements System.Windows.Data.IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim userid As String = TryCast(value, String)
        If Guid.TryParse(userid, New Guid) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class

#End Region

#Region "JsonConverters"
Class CustomIntConverter
    Inherits JsonConverter
    Public Overrides Function CanConvert(objectType As Type) As Boolean
        Return (objectType = GetType(Integer))
    End Function

    Public Overrides Function ReadJson(reader As JsonReader, objectType As Type, existingValue As Object, serializer As JsonSerializer) As Object
        Dim jsonValue As JValue = serializer.Deserialize(Of JValue)(reader)

        If jsonValue.Type = JTokenType.Float Then
            Return CInt(Math.Round(CDbl(jsonValue.Value)))
        ElseIf jsonValue.Type = JTokenType.[Integer] Then
            Return CInt(jsonValue.Value)
        End If

        Throw New FormatException()
    End Function

    Public Overrides Sub WriteJson(writer As JsonWriter, value As Object, serializer As JsonSerializer)
        Throw New NotImplementedException()
    End Sub
End Class

<ValueConversion(GetType(Boolean), GetType(Boolean))> _
Public Class InverseBooleanConverter
    Implements IValueConverter
#Region "IValueConverter Members"

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If targetType <> GetType(Boolean) Then
            Throw New InvalidOperationException("The target must be a boolean")
        End If

        Return Not CBool(value)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

#End Region
End Class


#End Region