import React, { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Users, Building2, FolderGit2, UserCheck, ChevronRight } from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import { usePublicTeams, useTeamsSummary } from '../hooks/useTeams';
import SearchInput from '../components/ui/SearchInput';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import Skeleton from '../components/ui/Skeleton';
import Badge from '../components/ui/Badge';
import Card from '../components/ui/Card';

export function TeamsPage() {
  const { t } = useTranslation(['organization', 'navigation', 'common']);
  const navigate = useNavigate();
  const { data: summary, isLoading: isSummaryLoading } = useTeamsSummary();
  const { data: teams, isLoading: isTeamsLoading, isError, refetch } = usePublicTeams();

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedDepartment, setSelectedDepartment] = useState<string>('ALL');

  // Extract unique departments for filter dropdown
  const departmentOptions = useMemo(() => {
    if (!teams) return [];
    const depts = new Set<string>();
    teams.forEach((team) => {
      if (team.departmentName) depts.add(team.departmentName);
    });
    return Array.from(depts).sort();
  }, [teams]);

  // Filter teams based on search query & department
  const filteredTeams = useMemo(() => {
    if (!teams) return [];
    return teams.filter((team) => {
      const matchesSearch =
        !searchQuery ||
        team.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (team.departmentName && team.departmentName.toLowerCase().includes(searchQuery.toLowerCase())) ||
        (team.description && team.description.toLowerCase().includes(searchQuery.toLowerCase()));

      const matchesDept = selectedDepartment === 'ALL' || team.departmentName === selectedDepartment;

      return matchesSearch && matchesDept;
    });
  }, [teams, searchQuery, selectedDepartment]);

  return (
    <PageLayout
      title={t('teams.title', { ns: 'organization' })}
      description={t('teams.subtitle', { ns: 'organization' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('teams.title', { ns: 'organization' }) },
      ]}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        {/* SUMMARY METRICS BAR */}
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: 'var(--space-4)',
          }}
        >
          <MetricCard
            icon={<Users size={20} style={{ color: '#f59e0b' }} />}
            title={t('teams.totalTeams', { ns: 'organization' })}
            value={summary?.totalTeams}
            loading={isSummaryLoading}
            bgColor="rgba(245, 158, 11, 0.12)"
            borderColor="rgba(245, 158, 11, 0.25)"
          />
          <MetricCard
            icon={<Building2 size={20} style={{ color: '#3b82f6' }} />}
            title={t('teams.totalDepartments', { ns: 'organization' })}
            value={summary?.totalDepartments}
            loading={isSummaryLoading}
            bgColor="rgba(59, 130, 246, 0.12)"
            borderColor="rgba(59, 130, 246, 0.25)"
          />
          <MetricCard
            icon={<FolderGit2 size={20} style={{ color: '#10b981' }} />}
            title={t('teams.activeTeams', { ns: 'organization' })}
            value={summary?.activeTeamsInPublishedProjects}
            loading={isSummaryLoading}
            bgColor="rgba(16, 185, 129, 0.12)"
            borderColor="rgba(16, 185, 129, 0.25)"
          />
          <MetricCard
            icon={<UserCheck size={20} style={{ color: '#8b5cf6' }} />}
            title={t('teams.membersInProjects', { ns: 'organization' })}
            value={summary?.totalMembersInProjects}
            loading={isSummaryLoading}
            bgColor="rgba(139, 92, 246, 0.12)"
            borderColor="rgba(139, 92, 246, 0.25)"
          />
        </div>

        {/* CONTROLS: SEARCH & FILTER */}
        <div
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 'var(--space-4)',
            alignItems: 'center',
            justifyContent: 'space-between',
            backgroundColor: 'var(--color-surface)',
            padding: 'var(--space-4)',
            borderRadius: 'var(--radius-lg)',
            border: '1px solid var(--color-border)',
          }}
        >
          <div style={{ flex: '1 1 300px', maxWidth: '480px' }}>
            <SearchInput
              className="search-input--light"
              value={searchQuery}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearchQuery(e.target.value)}
              onClear={() => setSearchQuery('')}
              placeholder={t('teams.searchPlaceholder', { ns: 'organization' })}
            />
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
            <label
              htmlFor="dept-filter"
              style={{
                fontSize: 'var(--font-size-sm)',
                fontWeight: 500,
                color: 'var(--color-text-muted)',
              }}
            >
              {t('teams.departmentLabel', { ns: 'organization' })}
            </label>
            <select
              id="dept-filter"
              value={selectedDepartment}
              onChange={(e) => setSelectedDepartment(e.target.value)}
              style={{
                padding: 'var(--space-2) var(--space-3)',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--color-border)',
                backgroundColor: 'var(--color-bg)',
                color: 'var(--color-text)',
                fontSize: 'var(--font-size-sm)',
                outline: 'none',
                cursor: 'pointer',
              }}
            >
              <option value="ALL">{t('teams.allDepartments', { ns: 'organization' })}</option>
              {departmentOptions.map((dept) => (
                <option key={dept} value={dept}>
                  {dept}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* TEAMS GRID / CONTENT */}
        {isTeamsLoading ? (
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
              gap: 'var(--space-4)',
            }}
          >
            {[1, 2, 3, 4, 5, 6].map((i) => (
              <Card key={i} style={{ height: '220px' }}>
                <Skeleton height="100%" />
              </Card>
            ))}
          </div>
        ) : isError ? (
          <ErrorState
            title={t('teams.loadErrorTitle', { ns: 'organization' })}
            description={t('teams.loadErrorDesc', { ns: 'organization' })}
            onRetry={refetch}
          />
        ) : filteredTeams.length === 0 ? (
          <EmptyState
            icon={<Users size={40} style={{ color: 'var(--color-text-muted)' }} />}
            title={t('teams.notFoundTitle', { ns: 'organization' })}
            description={t('teams.notFoundDesc', { ns: 'organization' })}
          />
        ) : (
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
              gap: 'var(--space-4)',
            }}
          >
            {filteredTeams.map((team) => (
              <Card
                key={team.id}
                onClick={() => navigate(`/teams/${team.id}`)}
                className="hover:border-amber-500/40 transition-all cursor-pointer"
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'space-between',
                  gap: 'var(--space-3)',
                }}
              >
                <div>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'flex-start',
                      gap: 'var(--space-2)',
                      marginBottom: 'var(--space-2)',
                    }}
                  >
                    <h3
                      style={{
                        fontSize: 'var(--font-size-base)',
                        fontWeight: 600,
                        color: 'var(--color-text)',
                        margin: 0,
                      }}
                    >
                      {team.name}
                    </h3>
                    {team.departmentName && (
                      <Badge variant="navy">
                        {team.departmentName}
                      </Badge>
                    )}
                  </div>

                  {team.description && (
                    <p
                      style={{
                        fontSize: 'var(--font-size-xs)',
                        color: 'var(--color-text-muted)',
                        margin: 0,
                        lineClamp: 2,
                        WebkitLineClamp: 2,
                        display: '-webkit-box',
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                        marginBottom: 'var(--space-3)',
                      }}
                    >
                      {team.description}
                    </p>
                  )}

                  {/* METRICS */}
                  <div
                    style={{
                      display: 'flex',
                      gap: 'var(--space-4)',
                      fontSize: 'var(--font-size-xs)',
                      color: 'var(--color-text-muted)',
                      marginBottom: 'var(--space-3)',
                    }}
                  >
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                      <FolderGit2 size={15} style={{ color: '#10b981', flexShrink: 0 }} />
                      <strong style={{ color: 'var(--color-text)' }}>{team.publishedProjectCount}</strong> {t('teams.publishedProjects', { ns: 'organization' })}
                    </span>
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                      <Users size={15} style={{ color: '#3b82f6', flexShrink: 0 }} />
                      <strong style={{ color: 'var(--color-text)' }}>{team.memberCount}</strong> {t('teams.memberCount', { ns: 'organization' })}
                    </span>
                  </div>

                  {/* TAGS: TECHNOLOGIES & LOCATIONS */}
                  {team.technologies && team.technologies.length > 0 && (
                    <div
                      style={{
                        display: 'flex',
                        flexWrap: 'wrap',
                        gap: 'var(--space-1)',
                        marginBottom: 'var(--space-2)',
                      }}
                    >
                      {team.technologies.map((tech) => (
                        <span
                          key={tech}
                          style={{
                            fontSize: '10px',
                            fontWeight: 500,
                            padding: '2px 6px',
                            borderRadius: 'var(--radius-sm)',
                            backgroundColor: 'rgba(234, 179, 8, 0.1)',
                            color: '#d97706',
                            border: '1px solid rgba(234, 179, 8, 0.2)',
                          }}
                        >
                          {tech}
                        </span>
                      ))}
                    </div>
                  )}

                  {team.locations && team.locations.length > 0 && (
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-1)' }}>
                      {team.locations.map((loc) => (
                        <span
                          key={loc}
                          style={{
                            fontSize: '10px',
                            fontWeight: 500,
                            padding: '2px 6px',
                            borderRadius: 'var(--radius-sm)',
                            backgroundColor: 'rgba(59, 130, 246, 0.08)',
                            color: '#2563eb',
                            border: '1px solid rgba(59, 130, 246, 0.15)',
                          }}
                        >
                          {loc}
                        </span>
                      ))}
                    </div>
                  )}
                </div>

                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'flex-end',
                    paddingTop: 'var(--space-2)',
                    borderTop: '1px dashed var(--color-border)',
                    fontSize: 'var(--font-size-xs)',
                    fontWeight: 600,
                    color: 'var(--color-primary)',
                  }}
                >
                  {t('teams.inspectTeam', { ns: 'organization' })}
                  <ChevronRight size={16} style={{ marginLeft: 4 }} />
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </PageLayout>
  );
}

interface MetricCardProps {
  icon: React.ReactNode;
  title: string;
  value?: number;
  loading?: boolean;
  bgColor?: string;
  borderColor?: string;
}

function MetricCard({ icon, title, value, loading, bgColor, borderColor }: MetricCardProps) {
  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        gap: 'var(--space-3)',
        padding: 'var(--space-4)',
        backgroundColor: 'var(--color-surface)',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--color-border)',
      }}
    >
      <div
        style={{
          width: 42,
          height: 42,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          borderRadius: 'var(--radius-md)',
          backgroundColor: bgColor || 'var(--color-bg)',
          border: `1px solid ${borderColor || 'var(--color-border)'}`,
          flexShrink: 0,
        }}
      >
        {icon}
      </div>
      <div>
        <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)', fontWeight: 500 }}>{title}</div>
        {loading ? (
          <Skeleton height="24px" width="60px" className="mt-1" />
        ) : (
          <div style={{ fontSize: 'var(--font-size-xl)', fontWeight: 700, color: 'var(--color-text)', marginTop: 2 }}>
            {value ?? 0}
          </div>
        )}
      </div>
    </div>
  );
}

export default TeamsPage;
