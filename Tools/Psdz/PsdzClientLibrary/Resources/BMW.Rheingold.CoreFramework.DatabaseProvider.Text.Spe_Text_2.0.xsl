<?xml version="1.0" encoding="UTF-8" ?>
<!-- =================================================================================================
	Last committed: $Revision: $
	Last changed by: $Author: $
	Last changed date: $Date: $
	================================================================================================= -->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:spe="http://bmw.com/2014/Spe_Text_2.0" >

	<xsl:output method="html" encoding="UTF-8" indent="no" omit-xml-declaration="yes" />

	<xsl:param name="fullHtml"></xsl:param>
	 <xsl:param name="lang">
		  <xsl:choose>
			<xsl:when test="string-length(@lang) > 0">
			  <xsl:value-of select="normalize-space(@lang)"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text>en-GB</xsl:text>
			</xsl:otherwise>
		  </xsl:choose>
	</xsl:param>

	 <xsl:variable name="hintWARNING">
		<xsl:text>
				de-DE:Warnung
				en-GB:Warning
        en-US:Warning
        el-GR:Warning
        id-ID:Warning
				es-ES:¡Advertencia
				fr-FR:Avertissement
				it-IT:Avvertenza
				ja-JP:警告
				ko-KR:경고
				nl-NL:Waarschuwing
				pt-PT:Advertência
				ru-RU:Предостережение
				sv-SE:Varning
				th-TH:ข้อควรระวัง
				tr-TR:Uyarı
				zh-CN:警告
        zh-TW:警告
				cs-CZ:Varování
				pl-PL:Ostrzeżenie
			</xsl:text>
	  </xsl:variable>
	  <xsl:variable name="hintHINT">
		<xsl:text>
				de-DE:Hinweis
				en-GB:Notice
        en-US:Notice
        el-GR:Notice
        id-ID:Notice
				es-ES:Indicación
				fr-FR:Remarque
				it-IT:Indicazione
				ja-JP:注意事項
				ko-KR:지침
				nl-NL:Let op
				pt-PT:Indicação
				ru-RU:Указание
				sv-SE:Anmärkning
				th-TH:หมายเหต
				tr-TR:Bilgi
				zh-CN:提示
        zh-TW:提示
				cs-CZ:Upozornění
				pl-PL:Wskazówki
			</xsl:text>
	  </xsl:variable>
	  <xsl:variable name="hintCAUTION">
		<xsl:text>
				de-DE:Vorsicht
				en-GB:Caution
        en-US:Caution
        el-GR:Caution
        id-ID:Caution
				es-ES:¡Atención
				fr-FR:Prudence
				it-IT:Attenzione
				ja-JP:注意
				ko-KR:주의
				nl-NL:Voorzichtig
				pt-PT:Cuidado
				ru-RU:Осторожно
				sv-SE:OBS
				th-TH:ข้อควรระวัง
				tr-TR:Dikkat
				zh-CN:小心
        zh-TW:小心
				cs-CZ:Opatrně
				pl-PL:Zachowaj ostrożność
			</xsl:text>
	  </xsl:variable>

	<xsl:template match="/">
		<xsl:choose>
			<xsl:when test="$fullHtml">
		<html>
			<head>
				<meta charset="utf-8" />
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<meta http-equiv="X-UA-Compatible" content="IE=8" />
				<meta http-equiv="cache-control" content="no-cache, no-store, must-revalidate"/>
				<meta http-equiv="expires" content="0"/>
				<meta http-equiv="pragma" content="no-cache"/>

			  <xsl:call-template name="html_css" />
			</head>
			<body>
				<xsl:apply-templates />
			</body>
		</html>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="DIV">
			<xsl:attribute name="class">
				<xsl:value-of select="local-name()" />
				<xsl:if test="(local-name() = 'LIST' or local-name() = 'SUBLIST') and @ORDERED = 'true'">
					<xsl:text> ORDERED</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="@internal_id">
			<xsl:attribute name="id">
				<xsl:value-of select="@internal_id" />
			</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates />
		</xsl:element>
	</xsl:template>

	<xsl:template match="spe:DIAGCODE/spe:CONTENT">
		DIAGCODE: <xsl:value-of select="text()"  disable-output-escaping="yes" />
	</xsl:template>

  <!-- Fix wrong sublist position: ignore here-->
  <xsl:template match="spe:SUBLIST"></xsl:template>

  <xsl:template match="spe:LISTENTRY">
		<div>
			<xsl:attribute name="class"><xsl:value-of select="local-name()"/></xsl:attribute>
			<xsl:if test="@internal_id">
			<xsl:attribute name="id"><xsl:value-of select="@internal_id"/></xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="(count(text()) + count(*)) > 0">
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:otherwise>
					<span class="PARAGRAPH_TEXT">
					<xsl:if test="@internal_id">
						<xsl:attribute name="id">
							<xsl:value-of select="@internal_id"/>
						</xsl:attribute>
						</xsl:if>
            <xsl:text disable-output-escaping="yes"></xsl:text>
          </span>
				</xsl:otherwise>
			</xsl:choose>
      <!-- Fix wrong sublist position: handle sublists here-->
      <xsl:for-each select="following-sibling::*[position()=1]">
        <xsl:if test="local-name() = 'SUBLIST'" >
          <xsl:element name="DIV">
            <xsl:attribute name="class">
              <xsl:value-of select="local-name()" />
              <xsl:if test="@ORDERED = 'true'">
                <xsl:text> ORDERED</xsl:text>
              </xsl:if>
            </xsl:attribute>
            <xsl:if test="@internal_id">
            <xsl:attribute name="id">
              <xsl:value-of select="@internal_id" />
            </xsl:attribute>
            </xsl:if>
            <xsl:apply-templates />
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </div>
	</xsl:template>

<xsl:template name="HintType">
		<xsl:param name="header"/>
		<xsl:param name="hinweistyp"/>
		<div>
			<xsl:attribute name="class"><xsl:value-of select="local-name()"/></xsl:attribute>
			<xsl:attribute name="id"><xsl:value-of select="@internal_id"/></xsl:attribute>
			<xsl:choose>
				<xsl:when test="$hinweistyp = 'Caution'">
					<div class="HEADER_ROW">
						<div class="info_icon">
							<div class="baseline_aligner">
								<div class="caution-icon icon-small" />
							</div>
						</div>
						<div class="SIGNALWORD">
							<div class="baseline_aligner">
								<xsl:value-of select="$header"/>
							</div>
						</div>
					</div>
				</xsl:when>
				<xsl:when test="$hinweistyp = 'Hint'">
					<div class="HEADER_ROW">
						<div class="info_icon">
							<div class="baseline_aligner">
								<div class="hint-icon icon-small" />
							</div>
						</div>
						<div class="SIGNALWORD">
							<div class="baseline_aligner">
								<xsl:value-of select="$header"/>
							</div>
						</div>
					</div>
				</xsl:when>
				<xsl:when test="$hinweistyp = 'Warning'">
					<div class="HEADER_ROW">
						<div class="info_icon">
							<div class="baseline_aligner">
								<div class="warning-icon icon-small" />
							</div>
						</div>
						<div class="SIGNALWORD">
							<div class="baseline_aligner">
								<xsl:value-of select="$header"/>
							</div>
						</div>
					</div>
				</xsl:when>
			</xsl:choose>
			<div class="Border js-text-container">
      <xsl:if test="@internal_id">
				<xsl:attribute name="id"><xsl:value-of select="@internal_id"/></xsl:attribute>
       </xsl:if>
				<xsl:apply-templates/>
			</div>
		</div>
	</xsl:template>

	<xsl:template name="text">
      <xsl:param name="var"/>
      <xsl:value-of select="substring-before(substring-after($var, concat($lang, ':')), '&#10;')"/>
    </xsl:template>

	<xsl:template match="spe:HINT">
		<xsl:variable name="varHINT">
		  <xsl:call-template name="text">
			<xsl:with-param name="var">
			  <xsl:value-of select="$hintHINT"/>
			</xsl:with-param>
		  </xsl:call-template>
		</xsl:variable>
		<xsl:call-template name="HintType">
			<xsl:with-param name="header"><xsl:value-of select="concat($varHINT, '!')" />
			</xsl:with-param>
			<xsl:with-param name="hinweistyp">Hint</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="spe:CAUTION">
		<xsl:variable name="varCAUTION">
        <xsl:call-template name="text">
          <xsl:with-param name="var">
            <xsl:value-of select="$hintCAUTION"/>
          </xsl:with-param>
        </xsl:call-template>
    </xsl:variable>
		<xsl:call-template name="HintType">
			<xsl:with-param name="header"><xsl:value-of select="concat($varCAUTION,'!')" /></xsl:with-param>
			<xsl:with-param name="hinweistyp">Caution</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="spe:WARNING">
		<xsl:variable name="varWARNING">
        <xsl:call-template name="text">
          <xsl:with-param name="var">
            <xsl:value-of select="$hintWARNING"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>
		<xsl:call-template name="HintType">
			<xsl:with-param name="header"><xsl:value-of select="concat($varWARNING,'!')" /></xsl:with-param>
			<xsl:with-param name="hinweistyp">Warning</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<xsl:template match="spe:PARAGRAPH/text()|spe:HINT/text()|spe:CAUTION/text()|spe:WARNING/text()|spe:ENTRY/text()|spe:LISTENTRY/text()">
		<span>
			<xsl:attribute name="class">PARAGRAPH_TEXT</xsl:attribute>
			<xsl:if test="../@internal_id">
			<xsl:attribute name="id">
				<xsl:value-of select="../@internal_id" />
			</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="normalize-space(.) and following-sibling::node()[1][self::spe:UNIT]">
					<xsl:call-template name="right-trim">
						<xsl:with-param name="s" select="."/>
					</xsl:call-template>
					<!-- non-breaking-space -->
					<xsl:text>&#160;</xsl:text>
				</xsl:when>
				<!-- trim left if first text in element -->
				<xsl:when test="count(preceding-sibling::node()) = 0">
					<xsl:call-template name="left-trim">
						<xsl:with-param name="s" select="."/>
					</xsl:call-template>
				</xsl:when>
				<!-- trim right if last text in element -->
				<xsl:when test="count(following-sibling::node()) = 0">
					<xsl:call-template name="right-trim">
						<xsl:with-param name="s" select="."/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>

	<xsl:template match="spe:EMPHASIZE1|spe:EMPHASIZE2|spe:NOWRAP|spe:EMPHASIZE1_NOWRAP|spe:EMPHASIZE2_NOWRAP">
		<span style="min-width: 5px;">
			<xsl:attribute name="class">
				<xsl:value-of select="local-name()" />
			</xsl:attribute>
			<xsl:if test="@internal_id">
			<xsl:attribute name="id">
				<xsl:value-of select="@internal_id" />
			</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="." />
		</span>
	</xsl:template>

	<xsl:template match="spe:UNIT|spe:SYMBOL">
		<span style="min-width: 5px;">
			<xsl:attribute name="class">
				<xsl:value-of select="local-name()" />
			</xsl:attribute>
			<xsl:if test="@internal_id">
			<xsl:attribute name="id">
				<xsl:value-of select="@internal_id" />
			</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="@REF" />
		</span>
	</xsl:template>

	<!-- standardtext ohne "flach-geklopftem" inhalt (z.B. standardtext innerhalb standardtext) -->
	<xsl:template match="spe:STANDARDTEXT[not(spe:CONTENT)]">
		<div class="STANDARDTEXT_REF">
			<span class="STANDARDTEXT_TITLE"><xsl:value-of select="@TITLE"/></span>
		</div>
	</xsl:template>

	<!-- standardtext mit "flach-geklopftem" inhalt in einer liste -->
	<!-- STANDARDTEXT => SIMPLE_TEXTITEM in list and table ==> use only PARAGRAPH/node() -->
    <xsl:template match="spe:LISTENTRY/spe:STANDARDTEXT[spe:CONTENT/spe:SIMPLE_TEXTITEM]|spe:ENTRY/spe:STANDARDTEXT[spe:CONTENT/spe:SIMPLE_TEXTITEM]|spe:PARAGRAPH/spe:STANDARDTEXT[spe:CONTENT/spe:SIMPLE_TEXTITEM]">
        <span class="STANDARDTEXT">
			<xsl:if test="@internal_id">
            <xsl:attribute name="id">
                <xsl:value-of select="@internal_id"/>
            </xsl:attribute>
			</xsl:if>
            <xsl:apply-templates select="./spe:CONTENT/spe:SIMPLE_TEXTITEM/spe:PARAGRAPH/node()"/>
        </span>
    </xsl:template>
	<!-- STANDARDTEXT => TEXTITEM in list and table ==> use only PARAGRAPH!!???-->
    <xsl:template match="spe:LISTENTRY/spe:STANDARDTEXT[spe:CONTENT/spe:TEXTITEM]|spe:ENTRY/spe:STANDARDTEXT[spe:CONTENT/spe:TEXTITEM]|spe:PARAGRAPH/spe:STANDARDTEXT[spe:CONTENT/spe:TEXTITEM]">
        <span class="STANDARDTEXT">
        <xsl:if test="@internal_id">
            <xsl:attribute name="id">
                <xsl:value-of select="@internal_id"/>
            </xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="./spe:CONTENT/spe:TEXTITEM/spe:PARAGRAPH"/>
        </span>
    </xsl:template>

	<!-- standardtext ohne "flach-geklopftem" inhalt in einer liste (z.B. einfacher standardtext in einer liste von einem komplexen standardtext) -->
	<xsl:template match="spe:LISTENTRY/spe:STANDARDTEXT[not(spe:CONTENT)]|spe:ENTRY/spe:STANDARDTEXT[not(spe:CONTENT)]|spe:PARAGRAPH/spe:STANDARDTEXT[not(spe:CONTENT)]">
		<span class="STANDARDTEXT_TITLE"><xsl:value-of select="@TITLE"/></span>
	</xsl:template>

	<xsl:template match="spe:PARAMETER">
		<span style="min-width: 5px;">
			<xsl:attribute name="class">
				<xsl:value-of select="local-name()" />
				<xsl:if test="@STATIC = 'true'">
                    <xsl:text> STATIC</xsl:text>
                </xsl:if>
            </xsl:attribute>
            <xsl:if test="@internal_id">
            <xsl:attribute name="id">
                <xsl:value-of select="@internal_id"/>
            </xsl:attribute>
            </xsl:if>
            <xsl:choose>
                <xsl:when test="string-length(@UNIT) > 0">
                    <xsl:call-template name="StringReplace">
                        <xsl:with-param name="input" select="concat(@ID, '&#160;', @UNIT)"/>
                        <xsl:with-param name="oldValue" select="' '"/>
                        <xsl:with-param name="newValue" select="'&#160;'"/>
                    </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="@ID"/>
                </xsl:otherwise>
            </xsl:choose>

        </span>
	</xsl:template>

    <xsl:template match="spe:VALUEUNIT">
   		<span style="min-width: 5px;">
            <xsl:attribute name="class">
                <xsl:value-of select="local-name()"/>
            </xsl:attribute>
            <xsl:if test="@internal_id">
   			<xsl:attribute name="id">
   				<xsl:value-of select="@internal_id" />
   			</xsl:attribute>
            </xsl:if>
            <xsl:call-template name="StringReplace">
                <xsl:with-param name="input" select="concat(@VALUE, '&#160;', @UNIT)"/>
                <xsl:with-param name="oldValue" select="' '"/>
                <xsl:with-param name="newValue" select="'&#160;'"/>
            </xsl:call-template>
   		</span>
   	</xsl:template>

    <xsl:template match="spe:TEXTPARAMETER">
   		<span style="min-width: 5px;">
   			<xsl:attribute name="class">
   				<xsl:value-of select="local-name()" />
                <xsl:text> STATIC</xsl:text>
   			</xsl:attribute>
   			<xsl:if test="@internal_id">
   			<xsl:attribute name="id">
   				<xsl:value-of select="@internal_id" />
   			</xsl:attribute>
            </xsl:if>
   			<xsl:value-of select="@ID" />
   		</span>
   	</xsl:template>

	<xsl:template name="left-trim">
		<xsl:param name="s" />
		<xsl:choose>
			<xsl:when test="substring($s, 1, 1) = ''">
				<xsl:value-of select="$s"/>
			</xsl:when>
			<xsl:when test="normalize-space(substring($s, 1, 1)) = ''">
				<xsl:call-template name="left-trim">
					<xsl:with-param name="s" select="substring($s, 2)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$s" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="right-trim">
		<xsl:param name="s"/>
		<xsl:choose>
			<xsl:when test="substring($s, 1, 1) = ''">
				<xsl:value-of select="$s"/>
			</xsl:when>
			<xsl:when test="normalize-space(substring($s, string-length($s))) = ''">
				<xsl:call-template name="right-trim">
					<xsl:with-param name="s" select="substring($s, 1, string-length($s) - 1)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$s" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

    <xsl:template name="StringReplace">
        <xsl:param name="input" />
        <xsl:param name="oldValue" />
        <xsl:param name="newValue" />
        <xsl:choose>
          <xsl:when test="contains($input, $oldValue)">
            <xsl:value-of select="substring-before($input,$oldValue)" />
            <xsl:value-of select="$newValue" />
            <xsl:call-template name="StringReplace">
              <xsl:with-param name="input"
              select="substring-after($input,$oldValue)" />
              <xsl:with-param name="oldValue" select="$oldValue" />
              <xsl:with-param name="newValue" select="$newValue" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$input" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:template>

	<xsl:template name="html_css">
		<style type="text/css">
			<xsl:comment>
		 		<xsl:text>
 /* reset all styles */
html, body, div, span,
h1, h2, h3, h4, h5, h6, p, blockquote, pre,
a, abbr, acronym, address, cite, code,
del, em, img, ins, kbd,
strong, sub, sup,
dl, dt, dd,
fieldset, form, label, legend,
table, caption, tbody, tfoot, thead, tr, th, td {
	margin: 0;
	padding: 0;
	border: 0;
	outline: 0;
	font-size: 100%;
	vertical-align: baseline;
	background: transparent;
}
body{
	line-height: 1;
	font: 10pt Helvetica;
	padding: 5px;
}
div{
	cursor: default;
}
span{
	cursor: default;
}

.TEXTITEM {
  /*border-top-width: 1px;
  border-top-style: solid;
  border-top-color: black;*/
  padding-top: 5px;
}

.TEXTITEMS {
    border-top-width: 3px;
    border-top-style: solid;
    border-top-color: black;
}

.TABLE {
	display: table;
	border-collapse: collapse;
	margin-bottom: 1em;
    margin-left: 5px;
}
.TTITLE {
	display: table-caption;
	font-weight: bold;
}
.THEAD  {
	display: table-header-group;
	background-color: #DDD;
}
.TBODY {
	display: table-row-group;
}
.ROW, .HEADROW {
	display: table-row;
}
.ENTRY, .HEADENTRY {
	display: table-cell;
	padding: 5px;
	border: 1px solid black;
}
.THEAD > .ROW > .ENTRY, .THEAD > .ROW > .HEADENTRY {
	font-weight: bold;
}
.DIAGCODE {
	padding: 5px;
	margin-bottom: 5px;
}
.PARAGRAPH {
	min-width:10px;
	padding: 5px;
	margin-bottom: 5px;
}

.TEXTCOLLECTION>.TITLE {
    font-size: 28px;
    margin: 10px 5px;
}

.TEXTCOLLECTION>.HINWEISTEXT {
   margin: 10px 5px;
}

.ENTRY>.STANDARDTEXT,
.ENTRY>.STANDARDTEXT>.PARAGRAPH_TEXT,
.PARAGRAPH>.STANDARDTEXT,
.PARAGRAPH>.STANDARDTEXT>.PARAGRAPH_TEXT {
    display: inline;
}

.ENTRY>.STANDARDTEXT>.PARAGRAPH_TEXT,
.PARAGRAPH>.STANDARDTEXT>.PARAGRAPH_TEXT {
    padding:       0;
    margin-bottom: 0;
}

.HINT .HEADER_ROW,
.WARNING .HEADER_ROW,
.CAUTION .HEADER_ROW {
	width: 100%;
	position: relative;
	height: 40px;
}

.HINT .info_icon,
.WARNING .info_icon,
.CAUTION .info_icon  {
	display: inline-block;
	position: absolute;
}

.HINT .baseline_aligner,
.WARNING .baseline_aligner,
.CAUTION .baseline_aligner {
	width: 100%;
	vertical-align: bottom;
	display: inline-block;
}

.HINT .SIGNALWORD,
.WARNING .SIGNALWORD,
.CAUTION .SIGNALWORD {
  font-size: 12pt;
  font-weight: bold;
  height: 40px;
  line-height: 40px;
  padding-left: 60px;
}

.HINT .SIGNALWORD,
.DAMAGE .SIGNALWORD {
  background-color: #0765af;
  color: #fff;
}

.WARNING .SIGNALWORD {
  background-color: #f6821f;
  color: #000;
}
.CAUTION .SIGNALWORD {
  background-color: #fef102;
  color: #000;
}
.DANGER .SIGNALWORD {
  background-color: #ee183a;
  color: #fff;
}

.HINT .PARAGRAPH_TEXT,
.CAUTION .PARAGRAPH_TEXT,
.WARNING .PARAGRAPH_TEXT {
	padding-left: 0;
	padding-top: 5px;
	padding-bottom: 5px;
}

.HINT .Border {
	border-bottom: 1px solid #0765af;
	border-left: 1px solid #0765af;
	border-right: 1px solid #0765af;
	margin-bottom: 16px;
	padding-top: 5px;
	padding-bottom: 5px;
	padding-left: 40px;
	padding-right: 40px;
}

.CAUTION .Border {
	border-bottom: 1px solid #fcff95;
	border-left: 1px solid #fcff95;
	border-right: 1px solid #fcff95; 
	margin-bottom: 16px;
	padding-top: 5px;
	padding-bottom: 5px;
	padding-left: 40px;
	padding-right: 40px;
}

.WARNING .Border {
	border-bottom: 1px solid #ffd8b8;
	border-left: 1px solid #ffd8b8;
	border-right: 1px solid #ffd8b8;
	margin-bottom: 16px;
	padding-top: 5px;
	padding-bottom: 5px;
	padding-left: 40px;
	padding-right: 40px;
}

.EMPHASIZE1, .EMPHASIZE1_NOWRAP {
	font-weight: bold;
}
.EMPHASIZE2, .EMPHASIZE2_NOWRAP {
	font-style: italic;
}
.NOWRAP, .EMPHASIZE1_NOWRAP, .EMPHASIZE2_NOWRAP {
    white-space: nowrap;
}


.PARAMETER, .TEXTPARAMETER{
	color: blue;
	/*font-weight: bold;
	border: 1px solid black;
	background-color: #b9ffff;
	padding-left: 5px;
	padding-right: 5px;*/
}
.TEXTPARAMETER {
    margin: 5px 5px 10px 5px;
    display: block;
}

.PARAMETER.STATIC,
.TEXTPARAMETER.STATIC {
	color: black;
	font-weight: normal;
}
.LIST {
	list-style: disc outside none;
	margin-left: 20px;
	margin-bottom: 10px;
}
.LIST.ORDERED {
    list-style-type: decimal;
    margin-left: 25px;
}
.SUBLIST {
	list-style: circle outside none;
	margin-left: 20px;
}
.SUBLIST.ORDERED {
    list-style-type: decimal;
     margin-left: 25px;
}
.LISTENTRY {
	display:list-item;
	margin-bottom: 5px;
}

.STANDARDTEXT_REF {
	padding: 5px;
	margin-bottom: 5px;
}
.STANDARDTEXT_TITLE {
	border: 1px solid black;
	background-color: lightyellow;
	padding-left: 5px;
	padding-right: 5px;
}

/****** Add Icons ******/
.icon-small {
	width: 40px;
	height: 40px;
	background-size: cover;
}
.caution-icon {
background-image:
url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACcAAAAnCAMAAAC7faEHAAABs1BMVEX98AB4cRn16QLt4ATk2QUqJh++tA7d0ghEPx6elhPc0Ae1qw/UyQlTTRyNhBbi1wYoJCBaVBzw4wOqoBHJvgvBtw3+8QDYzAnXzAg+OR6QhxWIgBfm2gbGvAvl2gUrJx+/tQ5/dxj87wHr3wRtZxoyLh/57QGUjBXx5AOLgxaCehf/8gCakRS5rw5xahru4gNPSh2pnxJoYRsjHyC3rQ89OB9VTxyupBDNwgrs4ARuaBpMRh2mnRJlXxv67gFTTR3LwAubkhRAOx6Tixbv4wOKghctKR9HQh6BeRjf0weAeRf98QAkICD16AKvpRDOwwrGvAzd0Qj77wFdVxx1bhk6NR7q3QRsZRqEfBdjXBuckxS7sQ7azwhBPB746wFbVR2TihXg1AclISCZkRQ9OB3u4gTl2QVeWByWjRR2bxk7Nh7z5wKupBHr3gRLRh0pJCBCPR7azgf57AGUixXSxwk5NB/x5QNqZBrCtw1hWxsmIiD36gLQxQrHvAuAeRhVUByPhxbs3wQzLh+nnhNDPh767QG1rBDTyAknIyDZzQhZUxw3Mx/v4gPIvQtpYxuhmBPf1AcKmXCuAAAACXBIWXMAAA9hAAAPYQGoP6dpAAABi0lEQVQ4jb3UZVPDQBAG4FDc/aC4W/BFinuxQEtxK+5SHFpcCsX5ydwlaXOlbdIZBvZL3t19PlwuM2FY74r5e4cMyBuHBpeLs7xwz1pYbFV2eaUAsKZSdGccdlCr5E7CCYOgMXmHGkGow35Zt3oM0J2ZowXtjJxjugBedlgUb4MRq4x7sgGE4qd1GDi9Z2c8xSe7IKkPoGbLo5smb3AkOohFHlzZLnGXJNbjMJrm3qFg/kamcWyeI6mQcesWcnl3j2PMLEmmMHcu60q44QmcU6L4OH7gxmksgmvCI434VQJc3blZ3Kk7WfZRzA0FLi5UXAE3xLI39uYT/XB1D/YVZLDMhz3fRTo7tO5gMM+ubDqaIsbJhaRLLloVJzUWHe0MG9IGuJZqqjMbKafnpIUpXxc/ScEqyQ1cU/MpPPItl/q9N4fbphgEkkkSNVhCost+pV0CccnUwP9LcCiCZtCDP2qvmp74MLxLtDg5rrJiqt158s67NlCqDt6lpiu5EuF8t/vy5Yf+5X/6K/cNckkmiOCLSt4AAAAASUVORK5CYII=')
}
.warning-icon {
background-image:
url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACcAAAAnCAMAAAC7faEHAAABtlBMVEVTMiDwfx8kHyDCaCCcVB/meh+4YyDcdR9kOiDScB9aNSB/RyArIiDJayBQMCDtfR/jeB9rPSC1YSA8KCC0YR9XMyB8RSCgVx9yQCCXUiC7ZB+xXx+DSCDMbB/xgB8lICBKLiDnex/ddh82JiBbNiDKbCCkWB/ufh/AZyDkeR89KSDQbx9YNCD1gR99RiApISDGah/rfB/Xch9fNyCoWx/NbR96RCBBKiDedR9mOiCLTCCvXh8tIiBSMiDvfx8jHyDAZh+RTx/2gh8qIiDsfR/ieB9qPSDYcx/zgB8nICDFaSDpex+7ZCBCKyBnOyDVcR8kICBJLiDBZx8/KSC3Yh+ISx9QMSCZUx/jeR+PTh88KSAyJCDPbx9XNCD0gR8oISChViBNLyCXUSCWUR9oPCA5JyBeNyDxfx/neh9AKiDddR+KTCCvXiAsIiC/Zh9iOCCGSh/1gh99RSDheB+OTSA6KCDXcx+oWh/ygB8mICDoex+6ZCCUUB/UcR8tIyDvfh9tPiDbdB/Rbx+jWCDHah90QSBFLCCPTiCzYB+FSSAxJCCpWx/zgR8nISDEaB9xPyCVUR84JyBu1xb/AAAACXBIWXMAAA9hAAAPYQGoP6dpAAABjUlEQVQ4jb3UZVPDQBAG4BIoUFyLuxYI7rYQrGhxJ8XdgheX4v6PuUvS5gJp0hkG7kve3X1mcneZiY5xb+n+3nFm2h1HP49/mN1w1+eQ9Kbtli4BID1Y08VSyEGpluscxgy6d9UdFwXCGqNV3eonwF15ay1sGNQc+wgQ2cLQJ+3wnqjiEl4B1tEz0Q+oTdeuvgftLBAnI0BdkUsXgU+QITrI5Fw4fR922TiGorCQquy4Ef5GwlA8ysNpm1V0bU+8m0YxZQUna5mSezEKN+yPcqWVj/MeCq7QJrgt9LYO8asM/nRZi+IsZ5lh1sWcfPzD1Ygj6LUwzJqj8OK+OVOFYwTeDFvlyBNXckc3OhnsMfp9Z5HPytxZgOSmgj2lwmYnnTlamgAVFE5UixeE26SkgfXe3lVCwDjJNeuIfgzakWlGqiv0TndIMLDgTjXROOVEF5JGulzs4olGwYHguCGSwQ76qMXkSeCW5d2kTeYo35vZAXmnn3dzoLX2eNfQpOUehP2NGtSXD/0v/9NfuS/zvoCHB4bSWAAAAABJRU5ErkJggg==')
}
.hint-icon {
background-image:
url('data:image/png;base64,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')
}

		 		</xsl:text>
			</xsl:comment>
		</style>
	</xsl:template>

</xsl:stylesheet>
