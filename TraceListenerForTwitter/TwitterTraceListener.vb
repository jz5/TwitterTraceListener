Imports CoreTweet

Public Class TwitterTraceListener
    Inherits TraceListener

    Private Const DefaultConsumerKey As String = "HtdvpRFxlG8ej9t4bMjYXrmEw"
    Private Const DefaultConsumerSecret As String = "MRS8jRTmpCicOgPnxmRK677LQeA6B0ELggg8s4RijuSKfYbbaf"

    Property ConsumerKey As String
    Property ConsumerSecret As String
    Property AccessToken As String
    Property AccessTokenSecret As String

    Private Tokens As Tokens

    Sub New()
    End Sub

    Sub New(accessToken As String, accessSecret As String)
        Me.AccessToken = accessToken
        Me.AccessTokenSecret = accessSecret
    End Sub

    Sub New(consumerKey As String, consumerSecret As String, accessToken As String, accessSecret As String)
        Me.ConsumerKey = consumerKey
        Me.ConsumerSecret = consumerSecret
        Me.AccessToken = accessToken
        Me.AccessTokenSecret = accessSecret
    End Sub

    Public Overrides Sub Write(message As String)
        Tweet(message)
    End Sub

    Public Overrides Sub WriteLine(message As String)
        Tweet(message)
    End Sub

    Private Function CreateTokens() As Tokens
        If Me.Tokens IsNot Nothing Then
            Return Me.Tokens
        End If

        Me.ConsumerKey = If(Me.ConsumerKey, If(Me.Attributes("consumerKey"), TwitterTraceListener.DefaultConsumerKey))
        Me.ConsumerSecret = If(Me.ConsumerSecret, If(Me.Attributes("consumerSecret"), TwitterTraceListener.DefaultConsumerSecret))
        Me.AccessToken = If(Me.AccessToken, Me.Attributes("accessToken"))
        Me.AccessTokenSecret = If(Me.AccessTokenSecret, Me.Attributes("accessSecret"))

        ' Create tokens
        If Not String.IsNullOrWhiteSpace(Me.AccessToken) AndAlso
               Not String.IsNullOrWhiteSpace(Me.AccessTokenSecret) Then

            Tokens = Tokens.Create(Me.ConsumerKey, Me.ConsumerSecret, Me.AccessToken, Me.AccessTokenSecret)
        Else
            ' If no access token/secret
            Try
                Dim session = OAuth.Authorize(ConsumerKey, ConsumerSecret)
                Process.Start(session.AuthorizeUri.AbsoluteUri) ' Open browser
                Dim pin = InputBox("Input PIN") ' Input PIN
                Me.Tokens = session.GetTokens(pin)

                Me.AccessToken = Tokens?.AccessToken
                Me.AccessTokenSecret = Tokens?.AccessTokenSecret
            Catch ex As Exception
                ' Do nothing
            End Try
        End If

        Return Me.Tokens
    End Function

    Private Sub Tweet(message As String)
        If message Is Nothing Then
            Exit Sub
        End If

        Me.Tokens = CreateTokens()
        If Me.Tokens Is Nothing Then
            Exit Sub
        End If

        If message.Length > 140 Then
            message = message.Substring(0, 137) & "..."
        End If
        Try
            tokens.Statuses.Update(message)
        Catch ex As Exception
            ' Do nothing
        End Try
    End Sub

    Protected Overrides Function GetSupportedAttributes() As String()
        Dim attrs = New List(Of String)(If(MyBase.GetSupportedAttributes(), New String() {}))
        attrs.Add("consumerKey")
        attrs.Add("consumerSecret")
        attrs.Add("accessToken")
        attrs.Add("accessSecret")

        Return attrs.ToArray
    End Function
End Class
