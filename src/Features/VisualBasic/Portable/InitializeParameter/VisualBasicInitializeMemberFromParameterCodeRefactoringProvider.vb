﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Threading
Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.InitializeParameter
Imports Microsoft.CodeAnalysis.LanguageServices
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.InitializeParameter
    <ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name:=NameOf(VisualBasicInitializeMemberFromParameterCodeRefactoringProvider)), [Shared]>
    <ExtensionOrder(Before:=NameOf(VisualBasicAddParameterCheckCodeRefactoringProvider))>
    Friend Class VisualBasicInitializeMemberFromParameterCodeRefactoringProvider
        Inherits AbstractInitializeMemberFromParameterCodeRefactoringProvider(Of
            ParameterSyntax,
            StatementSyntax,
            ExpressionSyntax)

        Protected Overrides Function IsFunctionDeclaration(node As SyntaxNode) As Boolean
            Return InitializeParameterHelpers.IsFunctionDeclaration(node)
        End Function

        Protected Overrides Function GetTypeBlock(node As SyntaxNode) As SyntaxNode
            Return DirectCast(node, TypeStatementSyntax).Parent
        End Function

        Protected Overrides Function TryGetLastStatement(blockStatement As IBlockOperation) As SyntaxNode
            Return InitializeParameterHelpers.TryGetLastStatement(blockStatement)
        End Function

        Protected Overrides Function GetBlockOperation(functionDeclaration As SyntaxNode, semanticModel As SemanticModel, cancellationToken As CancellationToken) As IBlockOperation
            Return InitializeParameterHelpers.GetBlockOperation(functionDeclaration, semanticModel, cancellationToken)
        End Function

        Protected Overrides Function IsImplicitConversion(compilation As Compilation, source As ITypeSymbol, destination As ITypeSymbol) As Boolean
            Return InitializeParameterHelpers.IsImplicitConversion(compilation, source, destination)
        End Function

        Protected Overrides Sub InsertStatement(editor As SyntaxEditor, functionDeclaration As SyntaxNode, method As IMethodSymbol, statementToAddAfterOpt As SyntaxNode, statement As StatementSyntax)
            InitializeParameterHelpers.InsertStatement(editor, functionDeclaration, statementToAddAfterOpt, statement)
        End Sub

        Protected Overrides Function DetermineDefaultFieldAccessibility(document As Document, functionDeclaration As SyntaxNode, model As SemanticModel, cancellationToken As CancellationToken) As Accessibility
            Dim accessibilityLevel = Accessibility.[Public]
            Dim service = document.GetLanguageService(Of ISyntaxFactsService)()
            Dim containingTypeNode = service.GetContainingTypeDeclaration(functionDeclaration, functionDeclaration.SpanStart)

            If containingTypeNode IsNot Nothing Then
                Dim containingTypeSymbol = CType(model.GetDeclaredSymbol(containingTypeNode, cancellationToken), INamedTypeSymbol)

                ' Fields are public by default in VB, except in the case of classes and modules.
                Select Case containingTypeSymbol.TypeKind
                    Case TypeKind.[Class], TypeKind.[Module]
                        accessibilityLevel = Accessibility.[Private]
                End Select
            End If
            Return accessibilityLevel
        End Function

        ' Properties are always public by default in VB.
        Protected Overrides Function DetermineDefaultPropertyAccessibility() As Accessibility
            Return Accessibility.[Public]
        End Function
    End Class
End Namespace
