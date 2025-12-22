<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="utf-8"/>

    <xsl:template match="/">
        <html>
            <head>
                <meta charset="utf-8"/>
                <title>Library</title>
                <style>
                    body { font-family: Arial; margin: 20px; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; }
                    th { background: #f2f2f2; }
                    .meta { margin-bottom: 12px; color: #555; }
                </style>
            </head>
            <body>
                <h1><xsl:value-of select="library/@name"/></h1>
                <div class="meta">Updated: <xsl:value-of select="library/@updated"/></div>

                <h2>Books</h2>
                <table>
                    <tr>
                        <th>ID</th>
                        <th>Author</th>
                        <th>Title</th>
                        <th>Year</th>
                        <th>ISBN</th>
                        <th>Type</th>
                        <th>Annotation</th>
                    </tr>

                    <xsl:for-each select="library/books/book">
                        <tr>
                            <td><xsl:value-of select="@id"/></td>
                            <td><xsl:value-of select="author/@fullName"/></td>
                            <td><xsl:value-of select="title"/></td>
                            <td><xsl:value-of select="@year"/></td>
                            <td><xsl:value-of select="@isbn"/></td>
                            <td><xsl:value-of select="qualification/@type"/></td>
                            <td><xsl:value-of select="annotation"/></td>
                        </tr>
                    </xsl:for-each>
                </table>

                <h2>Readers</h2>
                <table>
                    <tr>
                        <th>ID</th>
                        <th>Full name</th>
                        <th>Faculty</th>
                        <th>Department</th>
                        <th>Position</th>
                    </tr>
                    <xsl:for-each select="library/readers/reader">
                        <tr>
                            <td><xsl:value-of select="@id"/></td>
                            <td><xsl:value-of select="@fullName"/></td>
                            <td><xsl:value-of select="@faculty"/></td>
                            <td><xsl:value-of select="@department"/></td>
                            <td><xsl:value-of select="@position"/></td>
                        </tr>
                    </xsl:for-each>
                </table>

            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
