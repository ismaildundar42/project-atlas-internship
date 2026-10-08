import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Users,
  Building2,
  FolderGit2,
  Wrench,
  MapPin,
  ArrowLeft,
  Briefcase,
  ExternalLink,
  ShieldCheck,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import { useTeamDetail } from '../hooks/useTeams';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';
import Badge from '../components/ui/Badge';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';

export function TeamDetailPage() {
  const { t } = useTranslation(['organization', 'navigation', 'common']);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const teamId = Number(id);

  const { data: team, isLoading, isError, refetch } = useTeamDetail(teamId);

  if (isLoading) {
    return (
      <PageLayout
        title={t('teams.title', { ns: 'organization' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('teams.title', { ns: 'organization' }), href: '/teams' },
          { label: t('teams.loadingText', { ns: 'organization' }) },
        ]}
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
          <Skeleton height="180px" />
          <Skeleton height="300px" />
        </div>
      </PageLayout>
    );
  }

  if (isError || !team) {
    return (
      <PageLayout
        title={t('teams.teamNotFoundTitle', { ns: 'organization' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('teams.title', { ns: 'organization' }), href: '/teams' },
          { label: t('teams.errorText', { ns: 'organization' }) },
        ]}
      >
        <ErrorState
          title={t('teams.teamNotFoundTitle', { ns: 'organization' })}
          description={t('teams.teamNotFoundDesc', { ns: 'organization' })}
          onRetry={refetch}
        />
        <div style={{ marginTop: 'var(--space-4)', textAlign: 'center' }}>
          <Button variant="secondary" onClick={() => navigate('/teams')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            {t('teams.backToTeams', { ns: 'organization' })}
          </Button>
        </div>
      </PageLayout>
    );
  }

  return (
    <PageLayout
      title={team.name}
      description={
        team.departmentName
          ? t('teams.deptRoleDesc', { department: team.departmentName, ns: 'organization' })
          : t('teams.defaultRoleDesc', { ns: 'organization' })
      }
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('teams.title', { ns: 'organization' }), href: '/teams' },
        { label: team.name },
      ]}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        {/* HEADER CARD */}
        <Card
          style={{
            padding: 'var(--space-6)',
            backgroundColor: 'var(--color-surface)',
            borderRadius: 'var(--radius-lg)',
            border: '1px solid var(--color-border)',
          }}
        >
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 'var(--space-3)' }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', marginBottom: 'var(--space-2)' }}>
                  <h1 style={{ fontSize: 'var(--font-size-2xl)', fontWeight: 700, color: 'var(--color-text)', margin: 0 }}>
                    {team.name}
                  </h1>
                  {team.departmentName && (
                    <Badge variant="navy">
                      <Building2 className="w-3.5 h-3.5 mr-1" />
                      {team.departmentName}
                    </Badge>
                  )}
                </div>
                {team.description && (
                  <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-muted)', margin: 0, maxWidth: '800px' }}>
                    {team.description}
                  </p>
                )}
              </div>

              <Button variant="secondary" size="sm" onClick={() => navigate('/teams')}>
                <ArrowLeft className="w-4 h-4 mr-1.5" />
                {t('teams.allTeamsBtn', { ns: 'organization' })}
              </Button>
            </div>

            {/* METRICS ROW */}
            <div
              style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
                gap: 'var(--space-3)',
                paddingTop: 'var(--space-4)',
                borderTop: '1px solid var(--color-border)',
              }}
            >
              <MetricItem icon={<FolderGit2 className="w-4 h-4 text-emerald-500" />} label={t('teams.publishedProjectsCount', { ns: 'organization' })} value={team.publishedProjectCount} />
              <MetricItem icon={<Users className="w-4 h-4 text-blue-500" />} label={t('teams.membersInProjects', { ns: 'organization' })} value={team.memberCount} />
              <MetricItem icon={<Wrench className="w-4 h-4 text-amber-500" />} label={t('teams.techDiversity', { ns: 'organization' })} value={team.technologyCount ?? team.technologies.length} />
              <MetricItem icon={<MapPin className="w-4 h-4 text-purple-500" />} label={t('teams.locationDiversity', { ns: 'organization' })} value={team.locationCount ?? team.locations.length} />
            </div>
          </div>
        </Card>

        {/* PROJECTS SECTION */}
        <div>
          <h2 style={{ fontSize: 'var(--font-size-lg)', fontWeight: 600, color: 'var(--color-text)', marginBottom: 'var(--space-4)', display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
            <FolderGit2 className="w-5 h-5 text-emerald-500" />
            {t('teams.responsibleAndContributingProjects', { ns: 'organization' })} ({team.projects.length})
          </h2>

          {team.projects.length === 0 ? (
            <EmptyState
              icon={<FolderGit2 className="w-10 h-10 text-slate-400" />}
              title={t('teams.noPublishedProjects', { ns: 'organization' })}
              description={t('teams.noPublishedProjectsDesc', { ns: 'organization' })}
            />
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(340px, 1fr))', gap: 'var(--space-4)' }}>
              {team.projects.map((project) => (
                <Card
                  key={project.id}
                  onClick={() => navigate(`/projects/${project.slug}`)}
                  className="hover:border-emerald-500/40 transition-all cursor-pointer"
                  style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between', gap: 'var(--space-3)' }}
                >
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 'var(--space-2)', marginBottom: 'var(--space-2)' }}>
                      {project.isPrimaryTeam ? (
                        <Badge variant="success" icon={<ShieldCheck className="w-3 h-3" />}>
                          {t('teams.primaryTeamBadge', { ns: 'organization' })}
                        </Badge>
                      ) : (
                        <Badge variant="navy">
                          {t('teams.supportingTeamBadge', { ns: 'organization' })}
                        </Badge>
                      )}

                      {project.categoryName && (
                        <Badge variant="info">
                          {project.categoryName}
                        </Badge>
                      )}
                    </div>

                    <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, color: 'var(--color-text)', margin: '0 0 var(--space-2) 0' }}>
                      {project.name}
                    </h3>

                    {project.shortDescription && (
                      <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)', margin: 0, lineClamp: 2, WebkitLineClamp: 2, display: '-webkit-box', WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                        {project.shortDescription}
                      </p>
                    )}
                  </div>

                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingTop: 'var(--space-2)', borderTop: '1px dashed var(--color-border)', fontSize: 'var(--font-size-xs)' }}>
                    <span style={{ color: 'var(--color-text-muted)' }}>
                      {project.statusName || 'Aktif'}
                    </span>
                    <span style={{ color: 'var(--color-primary)', fontWeight: 600, display: 'inline-flex', alignItems: 'center' }}>
                      {t('teams.projectDetailLink', { ns: 'organization' })}
                      <ExternalLink className="w-3.5 h-3.5 ml-1" />
                    </span>
                  </div>
                </Card>
              ))}
            </div>
          )}
        </div>

        {/* TWO COLUMN GRID FOR TECHNOLOGIES & LOCATIONS */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: 'var(--space-4)' }}>
          {/* TECHNOLOGIES */}
          <Card style={{ padding: 'var(--space-5)' }}>
            <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, color: 'var(--color-text)', marginBottom: 'var(--space-3)', display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
              <Wrench className="w-4 h-4 text-amber-500" />
              {t('teams.usedTechnologies', { ns: 'organization' })}
            </h3>
            {team.technologies.length === 0 ? (
              <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)', margin: 0 }}>
                {t('teams.noTechRecorded', { ns: 'organization' })}
              </p>
            ) : (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-2)' }}>
                {team.technologies.map((tech) => (
                  <span
                    key={tech}
                    style={{
                      fontSize: 'var(--font-size-xs)',
                      fontWeight: 500,
                      padding: 'var(--space-1) var(--space-2.5)',
                      borderRadius: 'var(--radius-md)',
                      backgroundColor: 'rgba(234, 179, 8, 0.1)',
                      color: '#d97706',
                      border: '1px solid rgba(234, 179, 8, 0.25)',
                    }}
                  >
                    {tech}
                  </span>
                ))}
              </div>
            )}
          </Card>

          {/* LOCATIONS */}
          <Card style={{ padding: 'var(--space-5)' }}>
            <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, color: 'var(--color-text)', marginBottom: 'var(--space-3)', display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
              <MapPin className="w-4 h-4 text-purple-500" />
              {t('teams.locationsTitle', { ns: 'organization' })}
            </h3>
            {team.locations.length === 0 ? (
              <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)', margin: 0 }}>
                {t('teams.noLocationsRecorded', { ns: 'organization' })}
              </p>
            ) : (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-2)' }}>
                {team.locations.map((loc) => (
                  <span
                    key={loc}
                    style={{
                      fontSize: 'var(--font-size-xs)',
                      fontWeight: 500,
                      padding: 'var(--space-1) var(--space-2.5)',
                      borderRadius: 'var(--radius-md)',
                      backgroundColor: 'rgba(147, 51, 234, 0.08)',
                      color: '#9333ea',
                      border: '1px solid rgba(147, 51, 234, 0.2)',
                    }}
                  >
                    {loc}
                  </span>
                ))}
              </div>
            )}
          </Card>
        </div>

        {/* MEMBERS SECTION */}
        <div>
          <h2 style={{ fontSize: 'var(--font-size-lg)', fontWeight: 600, color: 'var(--color-text)', marginBottom: 'var(--space-4)', display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
            <Users className="w-5 h-5 text-blue-500" />
            {t('teams.collaboratedMembers', { ns: 'organization' })} ({team.members.length})
          </h2>

          {team.members.length === 0 ? (
            <EmptyState
              icon={<Users className="w-10 h-10 text-slate-400" />}
              title={t('teams.noMembersRecorded', { ns: 'organization' })}
              description={t('teams.noMembersRecordedDesc', { ns: 'organization' })}
            />
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 'var(--space-4)' }}>
              {team.members.map((member) => (
                <Card key={member.id} style={{ padding: 'var(--space-4)', display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
                    <div
                      style={{
                        width: '38px',
                        height: '38px',
                        borderRadius: 'var(--radius-full)',
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        color: 'var(--color-primary)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontWeight: 700,
                        fontSize: 'var(--font-size-sm)',
                        border: '1px solid rgba(59, 130, 246, 0.2)',
                      }}
                    >
                      {member.firstName?.[0]}
                      {member.lastName?.[0]}
                    </div>
                    <div>
                      <h4 style={{ fontSize: 'var(--font-size-sm)', fontWeight: 600, color: 'var(--color-text)', margin: 0 }}>
                        {member.firstName} {member.lastName}
                      </h4>
                      {member.title && (
                        <span style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)' }}>
                          {member.title}
                        </span>
                      )}
                    </div>
                  </div>

                  {member.projectRoles && member.projectRoles.length > 0 && (
                    <div style={{ marginTop: 'var(--space-1)', display: 'flex', flexWrap: 'wrap', gap: 'var(--space-1)' }}>
                      {member.projectRoles.map((role) => (
                        <Badge key={role} variant="navy" icon={<Briefcase className="w-2.5 h-2.5" />}>
                          {role}
                        </Badge>
                      ))}
                    </div>
                  )}
                </Card>
              ))}
            </div>
          )}
        </div>
      </div>
    </PageLayout>
  );
}

function MetricItem({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2.5)' }}>
      <div style={{ padding: 'var(--space-1.5)', borderRadius: 'var(--radius-sm)', backgroundColor: 'var(--color-bg)' }}>
        {icon}
      </div>
      <div>
        <div style={{ fontSize: '11px', color: 'var(--color-text-muted)' }}>{label}</div>
        <div style={{ fontSize: 'var(--font-size-sm)', fontWeight: 700, color: 'var(--color-text)' }}>{value}</div>
      </div>
    </div>
  );
}

export default TeamDetailPage;
