Imports System
Imports System.Text
Imports System.IO
Imports System.IO.Compression


Module Program

    Private br As BinaryReader
    Private des As String
    Private source As String
    Private buffer As Byte()

    Private ms As MemoryStream

    Sub Main(args As String())

        If args.Count = 0 Then
            Console.WriteLine("UnPack Tool - 2CongLc.vn")
        Else
            source = args(0)
        End If

        If File.Exists(source) Then

            br = New BinaryReader(File.OpenRead(source))
            des = Path.GetDirectoryName(source) + "\"

            br.BaseStream.Position = 26
            ms = New MemoryStream()
            Using ds As DeflateStream = New DeflateStream(New MemoryStream(br.ReadBytes(CInt(br.BaseStream.Length - 26))), CompressionMode.Decompress)
                ds.CopyTo(ms)
            End Using

            br = New BinaryReader(ms)
            br.BaseStream.Position = 0
            Dim subfiles As New List(Of FileData)()
            While br.BaseStream.Position < br.BaseStream.Length
                subfiles.Add(New FileData)
                Dim unknow As Byte() = br.ReadBytes(14)
            End While

            For Each fd As FileData In subfiles

                If fd.container < 9 Then
                    br = New BinaryReader(File.OpenRead(des + "WZ_0" + (fd.container + 1) + ".bin"))
                Else
                    br = New BinaryReader(File.OpenRead(des + "WZ_" + (fd.container + 1) + ".bin"))
                End If
                Console.WriteLine("File Offset : {0} - File SizeCompressed - File SizeUncompressed : {2} - File Name : {3}", fd.offset, fd.sizeCompressed, fd.sizeUncompressed, fd.name)
                br.BaseStream.Position = fd.offset
                Dim buffer As Byte()
                Directory.CreateDirectory(des + Path.GetFileNameWithoutExtension(source) + "\" + Path.GetDirectoryName(fd.name))

                If fd.isCompressed = 2 Then
                    buffer = br.ReadBytes(fd.sizeCompressed)
                    Dim fs As FileStream = File.Create(des + Path.GetFileNameWithoutExtension(source) + "\" + fd.name)
                    Dim unknow1 As Int16 = br.ReadInt16
                    Using dfs As New DeflateStream(New MemoryStream(buffer), CompressionMode.Decompress)
                        dfs.CopyTo(fs)
                    End Using
                    fs.Close()
                Else
                    buffer = br.ReadBytes(fd.sizeUncompressed)
                    Using bw As New BinaryWriter(File.Create(des + Path.GetFileNameWithoutExtension(source) + "\" + fd.name))
                        bw.Write(buffer)
                    End Using
                End If
            Next
            br.Close()
            Console.WriteLine("Unpack Done !!!")
        End If
        Console.ReadLine()
    End Sub

    ' Cấu trúc dữ liệu block
    Class FileData
        Public name As String = New String(br.ReadChars(260)).TrimEnd(ChrW(0))
        Public isCompressed As Byte = br.ReadByte()
        Public container As Byte = br.ReadByte()
        Public offset As Integer = br.ReadInt32() + 4
        Public sizeUncompressed As Integer = br.ReadInt32()
        Public sizeCompressed As Integer = br.ReadInt32()
        Public checksum As Single = br.ReadSingle()
        Public unknown As Integer = br.ReadInt32()
    End Class
End Module
