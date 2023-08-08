﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace amorphie.workflow.data.Migrations
{
    [DbContext(typeof(WorkflowDBContext))]
    [Migration("20230704085957_MigrationsTest6")]
    partial class MigrationsTest6
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Instance", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("BaseStatus")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("EntityName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RecordId")
                        .HasColumnType("uuid");

                    b.Property<string>("StateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WorkflowName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ZeebeFlowName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("StateName");

                    b.HasIndex("WorkflowName");

                    b.HasIndex("ZeebeFlowName");

                    b.HasIndex("EntityName", "RecordId", "StateName");

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("InstanceEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AdditionalData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ExecutedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("InputData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("InstanceTransitionId")
                        .HasColumnType("uuid");

                    b.Property<string>("OutputData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InstanceTransitionId");

                    b.ToTable("InstanceEvents");
                });

            modelBuilder.Entity("InstanceTransition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AdditionalData")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("EntityData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FormData")
                        .HasColumnType("text");

                    b.Property<string>("FromStateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("InstanceId")
                        .HasColumnType("uuid");

                    b.Property<string>("QueryData")
                        .HasColumnType("text");

                    b.Property<string>("RouteData")
                        .HasColumnType("text");

                    b.Property<string>("ToStateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransitionName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FromStateName");

                    b.HasIndex("InstanceId");

                    b.HasIndex("ToStateName");

                    b.HasIndex("TransitionName");

                    b.ToTable("InstanceTransitions");
                });

            modelBuilder.Entity("State", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("BaseStatus")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("OnEntryFlowName")
                        .HasColumnType("text");

                    b.Property<string>("OnExitFlowName")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("WorkflowName")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.HasIndex("OnEntryFlowName");

                    b.HasIndex("OnExitFlowName");

                    b.HasIndex("WorkflowName");

                    b.ToTable("States");

                });

            modelBuilder.Entity("Transition", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("FlowName")
                        .HasColumnType("text");

                    b.Property<string>("FromStateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("ServiceName")
                        .HasColumnType("text");

                    b.Property<string>("ToStateName")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.HasIndex("FlowName");

                    b.HasIndex("FromStateName");

                    b.HasIndex("ToStateName");

                    b.ToTable("Transitions");

                });

            modelBuilder.Entity("Workflow", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.Property<Guid?>("WorkflowEntityId")
                        .HasColumnType("uuid");

                    b.Property<int?>("WorkflowStatus")
                        .HasColumnType("integer");

                    b.Property<string>("ZeebeFlowName")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.HasIndex("WorkflowEntityId");

                    b.HasIndex("ZeebeFlowName");

                    b.ToTable("Workflows");


                });

            modelBuilder.Entity("WorkflowEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("AllowOnlyOneActiveInstance")
                        .HasColumnType("boolean");

                    b.Property<int>("AvailableInStatus")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsStateManager")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WorkflowName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("WorkflowName");

                    b.ToTable("WorkflowEntities");

                    
                });

            modelBuilder.Entity("ZeebeMessage", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("Gateway")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("Process")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.HasKey("Name");

                    b.ToTable("ZeebeMessages");

                   
                });

            modelBuilder.Entity("amorphie.core.Base.Translation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ModifiedByBehalfOf")
                        .HasColumnType("uuid");

                    b.Property<string>("StateName_Description")
                        .HasColumnType("text");

                    b.Property<string>("StateName_Title")
                        .HasColumnType("text");

                    b.Property<string>("TransitionName_Form")
                        .HasColumnType("text");

                    b.Property<string>("TransitionName_Page")
                        .HasColumnType("text");

                    b.Property<string>("TransitionName_Title")
                        .HasColumnType("text");

                    b.Property<string>("WorkflowName_Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("StateName_Description");

                    b.HasIndex("StateName_Title");

                    b.HasIndex("TransitionName_Form");

                    b.HasIndex("TransitionName_Page");

                    b.HasIndex("TransitionName_Title");

                    b.HasIndex("WorkflowName_Title");

                    b.ToTable("Translation");

                    
                });

            modelBuilder.Entity("Instance", b =>
                {
                    b.HasOne("State", "State")
                        .WithMany()
                        .HasForeignKey("StateName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Workflow", "Workflow")
                        .WithMany()
                        .HasForeignKey("WorkflowName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ZeebeMessage", "ZeebeFlow")
                        .WithMany()
                        .HasForeignKey("ZeebeFlowName");

                    b.Navigation("State");

                    b.Navigation("Workflow");

                    b.Navigation("ZeebeFlow");
                });

            modelBuilder.Entity("InstanceEvent", b =>
                {
                    b.HasOne("InstanceTransition", "InstanceTransition")
                        .WithMany()
                        .HasForeignKey("InstanceTransitionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InstanceTransition");
                });

            modelBuilder.Entity("InstanceTransition", b =>
                {
                    b.HasOne("State", "FromState")
                        .WithMany()
                        .HasForeignKey("FromStateName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Instance", "Instance")
                        .WithMany()
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("State", "ToState")
                        .WithMany()
                        .HasForeignKey("ToStateName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Transition", "Transition")
                        .WithMany()
                        .HasForeignKey("TransitionName");

                    b.Navigation("FromState");

                    b.Navigation("Instance");

                    b.Navigation("ToState");

                    b.Navigation("Transition");
                });

            modelBuilder.Entity("State", b =>
                {
                    b.HasOne("ZeebeMessage", "OnEntryFlow")
                        .WithMany()
                        .HasForeignKey("OnEntryFlowName");

                    b.HasOne("ZeebeMessage", "OnExitFlow")
                        .WithMany()
                        .HasForeignKey("OnExitFlowName");

                    b.HasOne("Workflow", "Workflow")
                        .WithMany("States")
                        .HasForeignKey("WorkflowName");

                    b.Navigation("OnEntryFlow");

                    b.Navigation("OnExitFlow");

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("Transition", b =>
                {
                    b.HasOne("ZeebeMessage", "Flow")
                        .WithMany()
                        .HasForeignKey("FlowName");

                    b.HasOne("State", "FromState")
                        .WithMany("Transitions")
                        .HasForeignKey("FromStateName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("State", "ToState")
                        .WithMany()
                        .HasForeignKey("ToStateName");

                    b.Navigation("Flow");

                    b.Navigation("FromState");

                    b.Navigation("ToState");
                });

            modelBuilder.Entity("Workflow", b =>
                {
                    b.HasOne("WorkflowEntity", null)
                        .WithMany("InclusiveWorkflows")
                        .HasForeignKey("WorkflowEntityId");

                    b.HasOne("ZeebeMessage", "ZeebeFlow")
                        .WithMany()
                        .HasForeignKey("ZeebeFlowName");

                    b.Navigation("ZeebeFlow");
                });

            modelBuilder.Entity("WorkflowEntity", b =>
                {
                    b.HasOne("Workflow", "Workflow")
                        .WithMany("Entities")
                        .HasForeignKey("WorkflowName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workflow");
                });

            modelBuilder.Entity("amorphie.core.Base.Translation", b =>
                {
                    b.HasOne("State", null)
                        .WithMany("Descriptions")
                        .HasForeignKey("StateName_Description");

                    b.HasOne("State", null)
                        .WithMany("Titles")
                        .HasForeignKey("StateName_Title");

                    b.HasOne("Transition", null)
                        .WithMany("Forms")
                        .HasForeignKey("TransitionName_Form");

                    b.HasOne("Transition", null)
                        .WithMany("Pages")
                        .HasForeignKey("TransitionName_Page");

                    b.HasOne("Transition", null)
                        .WithMany("Titles")
                        .HasForeignKey("TransitionName_Title");

                    b.HasOne("Workflow", null)
                        .WithMany("Titles")
                        .HasForeignKey("WorkflowName_Title");
                });

            modelBuilder.Entity("State", b =>
                {
                    b.Navigation("Descriptions");

                    b.Navigation("Titles");

                    b.Navigation("Transitions");
                });

            modelBuilder.Entity("Transition", b =>
                {
                    b.Navigation("Forms");

                    b.Navigation("Pages");

                    b.Navigation("Titles");
                });

            modelBuilder.Entity("Workflow", b =>
                {
                    b.Navigation("Entities");

                    b.Navigation("States");

                    b.Navigation("Titles");
                });

            modelBuilder.Entity("WorkflowEntity", b =>
                {
                    b.Navigation("InclusiveWorkflows");
                });
#pragma warning restore 612, 618
        }
    }
}
