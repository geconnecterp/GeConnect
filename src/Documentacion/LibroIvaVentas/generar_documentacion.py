"""Genera documentación local reproducible; no accede a SQL ni modifica la aplicación."""
from pathlib import Path
from html import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import letter
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import json

ROOT = Path(__file__).resolve().parent
FONT_DIR = Path('C:/Windows/Fonts')
pdfmetrics.registerFont(TTFont('DocArial', str(FONT_DIR / 'arial.ttf')))
pdfmetrics.registerFont(TTFont('DocArialBold', str(FONT_DIR / 'arialbd.ttf')))
pdfmetrics.registerFontFamily('DocArial', normal='DocArial', bold='DocArialBold', italic='DocArial', boldItalic='DocArialBold')
styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name='DocTitle', fontName='DocArialBold', fontSize=23, leading=28, spaceAfter=14, textColor=colors.black))
styles.add(ParagraphStyle(name='DocH2', fontName='DocArialBold', fontSize=13, leading=17, spaceBefore=12, spaceAfter=6, textColor=colors.black, keepWithNext=True))
styles.add(ParagraphStyle(name='DocBody', fontName='DocArial', fontSize=10.5, leading=15, spaceAfter=8, textColor=colors.HexColor('#292822')))
styles.add(ParagraphStyle(name='DocCell', fontName='DocArial', fontSize=9.1, leading=12, spaceAfter=0))
styles.add(ParagraphStyle(name='DocCellHead', parent=styles['DocCell'], fontName='DocArialBold', textColor=colors.white))
styles.add(ParagraphStyle(name='DocMeta', fontName='DocArial', fontSize=9, leading=12, spaceAfter=10, textColor=colors.HexColor('#62605a')))

def paragraph(text, style='DocBody'):
    return Paragraph(escape(text).replace('\n', '<br/>'), styles[style])

def footer(c, d):
    c.saveState()
    c.setFont('DocArial', 8)
    c.setFillColor(colors.HexColor('#62605a'))
    c.drawString(48, 28, 'GECO  |  Libro IVA Ventas  |  14 de octubre de 2026')
    c.drawRightString(letter[0] - 48, 28, str(d.page))
    c.restoreState()

def build(spec):
    output = ROOT / spec['filename']
    story = []
    for i, page in enumerate(spec['pages']):
        if i:
            story.append(PageBreak())
        story.append(paragraph(page['title'], 'DocTitle' if i == 0 else 'DocH2'))
        if i == 0:
            story.append(paragraph(spec['subtitle'], 'DocMeta'))
        for block in page['blocks']:
            kind = block['type']
            if kind in ('p', 'h2', 'meta'):
                story.append(paragraph(block['text'], {'p':'DocBody','h2':'DocH2','meta':'DocMeta'}[kind]))
            elif kind == 'steps':
                for n, text in enumerate(block['items'], 1):
                    story.append(paragraph(f'{n}. {text}'))
            elif kind == 'list':
                for text in block['items']:
                    story.append(paragraph('- ' + text))
            elif kind == 'table':
                data = [[paragraph(c, 'DocCellHead' if r == 0 else 'DocCell') for c in row] for r, row in enumerate(block['rows'])]
                widths = [w * (letter[0] - 96) for w in block['widths']]
                t = Table(data, colWidths=widths, repeatRows=1, hAlign='LEFT')
                t.setStyle(TableStyle([
                    ('BACKGROUND', (0,0),(-1,0), colors.HexColor('#363128')),
                    ('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white, colors.HexColor('#f5f4f0')]),
                    ('GRID',(0,0),(-1,-1),.5,colors.HexColor('#d9d9d9')),
                    ('VALIGN',(0,0),(-1,-1),'MIDDLE'),
                    ('LEFTPADDING',(0,0),(-1,-1),7),('RIGHTPADDING',(0,0),(-1,-1),7),
                    ('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7)
                ]))
                story.extend([t, Spacer(1,10)])
    doc = SimpleDocTemplate(str(output), pagesize=letter, rightMargin=48, leftMargin=48,
                            topMargin=42, bottomMargin=48, title=spec['title'], author='GECO')
    doc.build(story, onFirstPage=footer, onLaterPages=footer)
    print(output)

if __name__ == '__main__':
    for filename in ('manual_contenido.json', 'analisis_filtros_contenido.json'):
        build(json.loads((ROOT / filename).read_text(encoding='utf-8')))
