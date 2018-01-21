<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:a="attribute" xmlns:c="collection" xmlns:o="object"
    exclude-result-prefixes="msxsl a c o"
>
  <!-- read a PowerDesigner ConceptualModel (cdm) file -->
  <xsl:output method="xml" indent="yes"/>
  <xsl:key name="DI" match="o:DataItem" use="@Id"/>

  <xsl:template match="Model">
    <conceptual>
      <xsl:apply-templates select="o:RootObject/c:Children/o:Model"/>
    </conceptual>
  </xsl:template>

  <xsl:template match="o:Model">
    <model>
      <name>
        <xsl:value-of select="a:Name"/>
      </name>
      <entities>
        <xsl:apply-templates select="c:Entities/o:Entity"/>
      </entities>
    </model>
  </xsl:template>

  <xsl:template match="o:Entity">
    <entity>
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
      <attributes>
        <xsl:apply-templates select="c:Attributes/o:EntityAttribute"/>
      </attributes>
    </entity>
  </xsl:template>

  <xsl:template match="o:EntityAttribute">
    <attribute>
      <xsl:apply-templates select="c:DataItem/o:DataItem"/>
    </attribute>
  </xsl:template>

  <xsl:template match="o:DataItem[@Ref]">
    <!-- takes 1s execution time with a key -->
    <xsl:apply-templates select="key('DI', @Ref)"/>
  </xsl:template>

  <xsl:template match="o:xx-DataItem[@Ref]">
    <!-- takes 24s total execution time without a key -->
    <xsl:variable name="id">
      <xsl:value-of select="@Ref"/>
    </xsl:variable>
    <xsl:apply-templates select="/Model/o:RootObject/c:Children/o:Model/c:DataItems/o:DataItem[@Id = $id]"/>
  </xsl:template>

  <xsl:template match="o:DataItem[@Id]">
    <item>
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
    </item>
  </xsl:template>
</xsl:stylesheet>
