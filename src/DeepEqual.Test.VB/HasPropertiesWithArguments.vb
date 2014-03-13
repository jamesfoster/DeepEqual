Public Class HasPropertiesWithArguments

  Dim _example As String

  Property Example(arg As String) As String
    Get
      Return _example
    End Get
    Set(value As String)
      _example = value
    End Set
  End Property

End Class

