Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports MouseLock.Core

#End Region

Namespace Core

    ''' <summary>
    ''' Unit tests for <see cref="ProcessNameList"/>.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ProcessNameListTests

        <TestMethod>
        Public Sub Parse_SplitsTrimsAndRemovesEmptyEntries()
            CollectionAssert.AreEqual({"a.exe", "b.exe", "c.exe"},
                                      ProcessNameList.Parse(" a.exe ;;b.exe; ; c.exe;").ToArray())
        End Sub

        <TestMethod>
        Public Sub Parse_RemovesCaseInsensitiveDuplicates_KeepingFirstCasing()
            CollectionAssert.AreEqual({"Game.exe", "other.exe"},
                                      ProcessNameList.Parse("Game.exe;GAME.EXE;other.exe;game.exe").ToArray())
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow(";;; ;")>
        Public Sub Parse_EmptyInput_ReturnsEmptyList(text As String)
            Assert.AreEqual(0, ProcessNameList.Parse(text).Count)
        End Sub

        <TestMethod>
        Public Sub GetProcessNames_StripsExeExtension()
            CollectionAssert.AreEqual({"notepad", "My Game"},
                                      ProcessNameList.GetProcessNames("notepad.exe;My Game.EXE").ToArray())
        End Sub

        <TestMethod>
        Public Sub GetProcessNames_AcceptsNamesWithoutExtension()
            CollectionAssert.AreEqual({"notepad", "game"},
                                      ProcessNameList.GetProcessNames("notepad;game.exe").ToArray())
        End Sub

        <TestMethod>
        Public Sub GetProcessNames_RemovesDuplicatesAfterStripping()
            CollectionAssert.AreEqual({"notepad"},
                                      ProcessNameList.GetProcessNames("notepad.exe;NOTEPAD").ToArray())
        End Sub

        <TestMethod>
        Public Sub GetProcessNames_IgnoresBareExtension()
            Assert.AreEqual(0, ProcessNameList.GetProcessNames(".exe").Count)
        End Sub

        <DataTestMethod>
        <DataRow("notepad.exe", "notepad")>
        <DataRow("NOTEPAD.EXE", "NOTEPAD")>
        <DataRow("  notepad.exe  ", "notepad")>
        <DataRow("notepad", "notepad")>
        <DataRow("archive.exe.bak", "archive.exe.bak")>
        Public Sub ToProcessName_Cases(input As String, expected As String)
            Assert.AreEqual(expected, ProcessNameList.ToProcessName(input))
        End Sub

        <TestMethod>
        Public Sub ToProcessName_Nothing_Throws()
            Assert.ThrowsException(Of ArgumentNullException)(Sub() ProcessNameList.ToProcessName(Nothing))
        End Sub

        <TestMethod>
        Public Sub Contains_MatchesWholeEntriesOnly()
            ' Regression: v1.1 used String.Contains, so "pad.exe" was considered present in "notepad.exe".
            Assert.IsTrue(ProcessNameList.Contains("notepad.exe;game.exe", "NotePad.exe"))
            Assert.IsFalse(ProcessNameList.Contains("notepad.exe;game.exe", "pad.exe"))
            Assert.IsFalse(ProcessNameList.Contains("notepad.exe", "notepad.ex"))
        End Sub

        <TestMethod>
        Public Sub Add_AppendsWithoutLeadingSeparator()
            Assert.AreEqual("game.exe", ProcessNameList.Add("", "game.exe"))
            Assert.AreEqual("a.exe;game.exe", ProcessNameList.Add("a.exe", "game.exe"))
        End Sub

        <TestMethod>
        Public Sub Add_ExistingEntry_IsNotDuplicated()
            Assert.AreEqual("Game.exe", ProcessNameList.Add("Game.exe", "GAME.EXE"))
        End Sub

        <TestMethod>
        Public Sub Add_NormalizesTheList()
            Assert.AreEqual("a.exe;b.exe;c.exe", ProcessNameList.Add(";a.exe;;b.exe;", "c.exe"))
        End Sub

        <TestMethod>
        Public Sub Remove_RemovesWholeEntry_AndPreservesOtherEntriesCasing()
            ' Regression: v1.1 removed by substring replacement and lowercased the whole list,
            ' so removing "pad.exe" turned "NotePad.exe;pad.exe" into "note;".
            Assert.AreEqual("NotePad.exe", ProcessNameList.Remove("NotePad.exe;pad.exe", "PAD.EXE"))
        End Sub

        <TestMethod>
        Public Sub Remove_MissingEntry_KeepsList()
            Assert.AreEqual("a.exe;b.exe", ProcessNameList.Remove("a.exe;b.exe", "c.exe"))
        End Sub

        <TestMethod>
        Public Sub Remove_LastEntry_ReturnsEmptyString()
            Assert.AreEqual(String.Empty, ProcessNameList.Remove("a.exe", "a.exe"))
        End Sub

        <DataTestMethod>
        <DataRow(Nothing)>
        <DataRow("")>
        <DataRow("   ")>
        <DataRow("a.exe;b.exe")>
        Public Sub EntryOperations_InvalidEntry_Throw(entry As String)
            Assert.ThrowsException(Of ArgumentException)(Sub() ProcessNameList.Add("x.exe", entry))
            Assert.ThrowsException(Of ArgumentException)(Sub() ProcessNameList.Remove("x.exe", entry))
            Assert.ThrowsException(Of ArgumentException)(Sub() ProcessNameList.Contains("x.exe", entry))
        End Sub

    End Class

End Namespace
