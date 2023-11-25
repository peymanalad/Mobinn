using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class GetListOfUsersDDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the procedure if it already exists
            migrationBuilder.Sql(@"
            IF OBJECT_ID('GetListOfUsers', 'P') IS NOT NULL
                DROP PROCEDURE GetListOfUsers;
        ");

            // Create the updated procedure
            migrationBuilder.Sql(@"
            ALTER PROCEDURE [dbo].[GetListOfUsers]
                @NationalIdFilter NVARCHAR(MAX) = NULL,
                @NameFilter NVARCHAR(MAX) = NULL,
                @SurNameFilter NVARCHAR(MAX) = NULL,
                @UserNameFilter NVARCHAR(MAX) = NULL,
                @PhoneNumberFilter NVARCHAR(MAX) = NULL,
                @IsActiveFilter BIT = NULL,
                @FromCreationDate DATETIME = NULL,
                @ToCreationDate DATETIME = NULL,
                @FromLastLoginDate DATETIME = NULL,
                @ToLastLoginDate DATETIME = NULL,
                @Role INT = NULL,
                @OnlyLockedUsers BIT = NULL,
                @Filter NVARCHAR(MAX) = NULL,
                @Permissions NVARCHAR(MAX) = NULL,
                @OrganizationId INT = NULL,
                @Sorting NVARCHAR(MAX) = NULL,
                @MaxResultCount INT = NULL,
                @SkipCount INT = 0
            AS
            BEGIN
                DECLARE @Sql NVARCHAR(MAX)

                SET @Sql = '
                WITH AbpUserLoginAttemptsRanked AS (
                    SELECT
                        u.Id,
                        u.NationalId,
                        u.Name,
                        u.Surname,
                        u.UserName,
                        u.PhoneNumber,
                        u.IsActive,
                        u.CreationTime,
                        u.EmailAddress,
                        u.ProfilePictureId,
                        u.LockoutEndDateUtc AS LockoutEndDate,
                        la.CreationTime AS LastLoginAttemptTime,
                        ROW_NUMBER() OVER (PARTITION BY u.Id ORDER BY la.CreationTime DESC) AS RowNum
                    FROM
                        AbpUsers u
                    LEFT JOIN
                        AbpUserLoginAttempts la ON u.Id = la.UserId
                    WHERE IsDeleted=0'

                -- Example: Add a condition based on @NationalIdFilter
                IF @NationalIdFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.NationalId = @NationalIdFilter'

                -- Add conditions for other filters
                IF @NameFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.Name = @NameFilter'

                IF @SurNameFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.Surname = @SurNameFilter'

                IF @UserNameFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.UserName = @UserNameFilter'

                IF @PhoneNumberFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.PhoneNumber = @PhoneNumberFilter'

                IF @IsActiveFilter IS NOT NULL
                    SET @Sql = @Sql + ' AND u.IsActive = @IsActiveFilter'

                IF @FromCreationDate IS NOT NULL AND @ToCreationDate IS NOT NULL
                    SET @Sql = @Sql + ' AND u.CreationTime BETWEEN @FromCreationDate AND @ToCreationDate'

                IF @FromLastLoginDate IS NOT NULL AND @ToLastLoginDate IS NOT NULL
                    SET @Sql = @Sql + ' AND la.CreationTime BETWEEN @FromLastLoginDate AND @ToLastLoginDate'

                IF @Role IS NOT NULL
                    SET @Sql = @Sql + ' AND u.Role = @Role'

                -- Add a condition based on @Filter
                IF @Filter IS NOT NULL
                    SET @Sql = @Sql + ' AND (u.NationalId LIKE ''%'' + @Filter + ''%'' OR u.Name LIKE ''%'' + @Filter + ''%'' OR u.Surname LIKE ''%'' + @Filter + ''%'' OR u.UserName LIKE ''%'' + @Filter + ''%'' OR u.PhoneNumber LIKE ''%'' + @Filter + ''%'')'

                SET @Sql = @Sql + ')
                
                SELECT
                    Id,
                    NationalId,
                    Name,
                    Surname,
                    UserName,
                    PhoneNumber,
                    IsActive,
                    CreationTime,
                    EmailAddress,
                    ProfilePictureId,
                    LockoutEndDate,
                    LastLoginAttemptTime
                FROM
                    AbpUserLoginAttemptsRanked
                WHERE
                    RowNum = 1'

                IF @Permissions IS NOT NULL
                BEGIN
                    SET @Sql = @Sql + '
                    AND EXISTS (
                        SELECT 1
                        FROM UserPermissions up
                        WHERE up.UserId = AbpUserLoginAttemptsRanked.Id
                            AND up.Name IN (''' + REPLACE(@Permissions, ',', ''',''') + ''')
                            AND up.IsGranted = 1
                    )'
                END

                IF @OrganizationId IS NOT NULL
                BEGIN
                    SET @Sql = @Sql + '
                    AND EXISTS (
                        SELECT 1
                        FROM GroupMembers gm
                        WHERE gm.UserId = AbpUserLoginAttemptsRanked.Id AND gm.OrganizationId = @OrganizationId
                    )'
                END

                IF @Sorting IS NOT NULL
                    SET @Sql = @Sql + '
                    ORDER BY ' + @Sorting

                IF @MaxResultCount IS NOT NULL
                    SET @Sql = @Sql + '
                    OFFSET ' + CAST(@SkipCount AS NVARCHAR(MAX)) + ' ROWS
                    FETCH NEXT ' + CAST(@MaxResultCount AS NVARCHAR(MAX)) + ' ROWS ONLY'

                EXEC sp_executesql @Sql, N'@NationalIdFilter NVARCHAR(MAX), @NameFilter NVARCHAR(MAX), @SurNameFilter NVARCHAR(MAX), @UserNameFilter NVARCHAR(MAX), @PhoneNumberFilter NVARCHAR(MAX), @IsActiveFilter BIT, @FromCreationDate DATETIME, @ToCreationDate DATETIME, @FromLastLoginDate DATETIME, @ToLastLoginDate DATETIME, @Role INT, @OnlyLockedUsers BIT, @Filter NVARCHAR(MAX), @Permissions NVARCHAR(MAX), @OrganizationId INT, @Sorting NVARCHAR(MAX)', @NationalIdFilter, @NameFilter, @SurNameFilter, @UserNameFilter, @PhoneNumberFilter, @IsActiveFilter, @FromCreationDate, @ToCreationDate, @FromLastLoginDate, @ToLastLoginDate, @Role, @OnlyLockedUsers, @Filter, @Permissions, @OrganizationId, @Sorting
            END;
        ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
