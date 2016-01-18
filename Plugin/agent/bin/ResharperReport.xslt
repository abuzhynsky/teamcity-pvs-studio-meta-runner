<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:output method="xml" indent="yes" encoding="UTF-8"/>
    <xsl:key name="project" match="PVS-Studio_Analysis_Log" use="Project" />
    <xsl:key name="errorCode" match="PVS-Studio_Analysis_Log" use="ErrorCode" />
    <xsl:variable name="issueTypes" select="document('PvsStudioIssueTypes.xml')" />
    <xsl:template match="/">
        <Report ToolsVersion="8.2">
            <Information>
                <Solution>
                    <xsl:value-of select="/NewDataSet/Solution_Path/SolutionPath"/>
                </Solution>
                <InspectionScope>
                    <Element>Solution</Element>
                </InspectionScope>
            </Information>
            <xsl:apply-templates select="NewDataSet/PVS-Studio_Analysis_Log[generate-id(.)=generate-id(key('project',Project)[1])]" />
            <IssueTypes>
                <xsl:apply-templates select="NewDataSet/PVS-Studio_Analysis_Log[generate-id(.)=generate-id(key('errorCode',ErrorCode)[1])]/ErrorCode" />
            </IssueTypes>
        </Report>

    </xsl:template>

    <xsl:template match="PVS-Studio_Analysis_Log">
        <Issues>
            <Project>
                <xsl:attribute name="name">
                    <xsl:value-of select="Project" />
                </xsl:attribute>
                <xsl:for-each select="key('project', Project)">
                    <xsl:element name="Issue">
                        <xsl:attribute name="TypeId">
                            <xsl:value-of select="ErrorCode" />
                        </xsl:attribute>
                        <xsl:attribute name="File">
							<xsl:value-of select="Project"/>
							<xsl:value-of select="'/'"/>
							<xsl:value-of select="ShortFile"/>
                        </xsl:attribute>
                        <xsl:attribute name="Offset">
                            <xsl:text>1</xsl:text>
                        </xsl:attribute>
                        <xsl:attribute name="Line">
                            <xsl:value-of select="Line" />
                        </xsl:attribute>
                        <xsl:attribute name="Message">
                            <xsl:value-of select="Message" />
                        </xsl:attribute>
                    </xsl:element>
                </xsl:for-each>
            </Project>
        </Issues>
    </xsl:template>

    <xsl:template match="ErrorCode">
        <xsl:variable name="error-code" select="text()"/>
        <xsl:copy-of select="$issueTypes/IssueTypes/IssueType[@Id=$error-code]"/>
    </xsl:template>
</xsl:stylesheet>
