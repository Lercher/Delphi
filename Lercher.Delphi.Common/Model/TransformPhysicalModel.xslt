<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:a="attribute" xmlns:c="collection" xmlns:o="object"
    exclude-result-prefixes="msxsl a c o"
>
    <!-- read a PowerDesigner PhysicalModel (pdm) file -->
    <xsl:output method="xml" indent="yes"/>

  <xsl:template match="Model">
    <physical>
      <xsl:apply-templates select="o:RootObject/c:Children/o:Model"/>
    </physical>
  </xsl:template>
                
  <xsl:template match="o:Model">
    <model>
      <name>
        <xsl:value-of select="a:Name"/>
      </name>
      <tables>
        <xsl:apply-templates select="c:Tables/o:Table"/>
      </tables>
    </model>
  </xsl:template>
  
  <xsl:template match="o:Table">
    <table>
      <name>
        <xsl:value-of select="a:Code"/>
      </name>
      <comment>
        <xsl:value-of select="a:Comment"/>
      </comment>
      <annotation>
        <xsl:value-of select="a:Annotation"/>
      </annotation>
      <description-rtf>
        <xsl:value-of select="a:Description"/>
      </description-rtf>
      <fields>
        <xsl:apply-templates select="c:Columns/o:Column"/>
      </fields>
    </table>
  </xsl:template>

  <xsl:template match="o:Column">
    <field>
      <name>
        <xsl:value-of select="a:Code"/>
      </name>
      <type>
        <xsl:value-of select="a:DataType"/>
      </type>
      <comment>
        <xsl:value-of select="a:Name"/>
      </comment>
      <annotation>
        <xsl:value-of select="a:Annotation"/>
      </annotation>
    </field>
  </xsl:template>
</xsl:stylesheet>
