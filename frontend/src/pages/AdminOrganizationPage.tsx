import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Building2,
  Users,
  UserCheck,
  Plus,
  Edit2,
  Trash2,
  X,
  AlertTriangle,
  CheckCircle2,
  FolderGit2,
  Shield,
  KeyRound,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import {
  useAdminDepartments,
  useAdminTeams,
  useAdminMembers,
  useCreateDepartment,
  useUpdateDepartment,
  useDeleteDepartment,
  useCreateTeam,
  useUpdateTeam,
  useDeleteTeam,
  useCreateMember,
  useUpdateMember,
  useDeleteMember,
  useUpdateMemberProjectAccess,
  useCreateMemberAccount,
  useLinkMemberAccount,
  useUpdateMemberAdminRole,
} from '../hooks/useAdminOrganization';
import { useAuth } from '../hooks/useAuth';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';
import Button from '../components/ui/Button';
import Card from '../components/ui/Card';
import type {
  DepartmentAdmin,
  TeamAdmin,
  MemberAdmin,
} from '../types/organization';

type TabType = 'teams' | 'members' | 'departments';

export function AdminOrganizationPage() {
  const { t, i18n } = useTranslation(['organization', 'navigation', 'common']);
  const [activeTab, setActiveTab] = useState<TabType>('teams');

  // Alert State
  const [toastMessage, setToastMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Modal State
  const [modalMode, setModalMode] = useState<'create' | 'edit' | null>(null);
  const [editingItem, setEditingItem] = useState<any | null>(null);

  // Delete Confirm State
  const [deleteConfirmItem, setDeleteConfirmItem] = useState<{ type: TabType; id: number; name: string } | null>(null);

  // Data Queries
  const { data: departments, isLoading: isDeptsLoading, isError: isDeptsError, refetch: refetchDepts } = useAdminDepartments();
  const { data: teams, isLoading: isTeamsLoading, isError: isTeamsError, refetch: refetchTeams } = useAdminTeams();
  const { data: members, isLoading: isMembersLoading, isError: isMembersError, refetch: refetchMembers } = useAdminMembers();

  // Mutations
  const createDeptMut = useCreateDepartment();
  const updateDeptMut = useUpdateDepartment();
  const deleteDeptMut = useDeleteDepartment();

  const createTeamMut = useCreateTeam();
  const updateTeamMut = useUpdateTeam();
  const deleteTeamMut = useDeleteTeam();

  const createMemberMut = useCreateMember();
  const updateMemberMut = useUpdateMember();
  const deleteMemberMut = useDeleteMember();

  const updateMemberProjectAccessMut = useUpdateMemberProjectAccess();
  const createMemberAccountMut = useCreateMemberAccount();
  const linkMemberAccountMut = useLinkMemberAccount();
  const updateMemberAdminRoleMut = useUpdateMemberAdminRole();

  const showToast = (type: 'success' | 'error', text: string) => {
    setToastMessage({ type, text });
    setTimeout(() => setToastMessage(null), 5000);
  };

  const handleOpenCreate = () => {
    setEditingItem(null);
    setModalMode('create');
  };

  const handleOpenEdit = (item: any) => {
    setEditingItem(item);
    setModalMode('edit');
  };

  const handleCloseModal = () => {
    setModalMode(null);
    setEditingItem(null);
  };

  const handleDelete = async () => {
    if (!deleteConfirmItem) return;
    const { type, id, name } = deleteConfirmItem;
    setDeleteConfirmItem(null);

    try {
      if (type === 'departments') {
        await deleteDeptMut.mutateAsync(id);
        showToast('success', `"${name}" ${i18n.language === 'en' ? 'department was deleted successfully.' : 'departmanı başarıyla silindi.'}`);
      } else if (type === 'teams') {
        await deleteTeamMut.mutateAsync(id);
        showToast('success', `"${name}" ${i18n.language === 'en' ? 'team was deleted successfully.' : 'ekibi başarıyla silindi.'}`);
      } else if (type === 'members') {
        await deleteMemberMut.mutateAsync(id);
        showToast('success', `"${name}" ${i18n.language === 'en' ? 'member was deleted successfully.' : 'üyesi başarıyla silindi.'}`);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'An error occurred during deletion.' : 'Silme işlemi sırasında hata oluştu.');
      showToast('error', msg);
    }
  };

  return (
    <PageLayout
      title={t('admin.title', { ns: 'organization' })}
      description={t('admin.subtitle', { ns: 'organization' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
        { label: t('admin.title', { ns: 'organization' }) },
      ]}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        {/* TOAST ALERTS */}
        {toastMessage && (
          <div
            style={{
              padding: 'var(--space-3) var(--space-4)',
              borderRadius: 'var(--radius-md)',
              backgroundColor: toastMessage.type === 'success' ? 'rgba(34, 197, 94, 0.1)' : 'rgba(239, 68, 68, 0.1)',
              border: `1px solid ${toastMessage.type === 'success' ? 'rgba(34, 197, 94, 0.3)' : 'rgba(239, 68, 68, 0.3)'}`,
              color: toastMessage.type === 'success' ? '#166534' : '#991b1b',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              fontSize: 'var(--font-size-sm)',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
              {toastMessage.type === 'success' ? <CheckCircle2 className="w-5 h-5" /> : <AlertTriangle className="w-5 h-5" />}
              <span>{toastMessage.text}</span>
            </div>
            <button
              onClick={() => setToastMessage(null)}
              style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'inherit' }}
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        )}

        {/* TOP BAR: STABLE SEGMENTED TABS & ACTION BUTTON AREA */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 'var(--space-4)',
            backgroundColor: 'var(--color-surface)',
            padding: 'var(--space-3) var(--space-4)',
            borderRadius: 'var(--radius-lg)',
            border: '1px solid var(--color-border)',
          }}
        >
          {/* SEGMENTED TAB LIST */}
          <div
            role="tablist"
            aria-label="Organizasyon modülleri"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              backgroundColor: 'var(--color-bg)',
              padding: '4px',
              borderRadius: 'var(--radius-md)',
              border: '1px solid var(--color-border)',
              gap: '4px',
            }}
          >
            <TabButton
              active={activeTab === 'teams'}
              onClick={() => setActiveTab('teams')}
              icon={<Users className="w-4 h-4" />}
              label={t('admin.tabs.teams', { ns: 'organization' })}
              count={teams?.length}
            />
            <TabButton
              active={activeTab === 'members'}
              onClick={() => setActiveTab('members')}
              icon={<UserCheck className="w-4 h-4" />}
              label={t('admin.tabs.members', { ns: 'organization' })}
              count={members?.length}
            />
            <TabButton
              active={activeTab === 'departments'}
              onClick={() => setActiveTab('departments')}
              icon={<Building2 className="w-4 h-4" />}
              label={i18n.language === 'en' ? 'Departments' : 'Departmanlar'}
              count={departments?.length}
            />
          </div>

          {/* STABLE RIGHT ACTION BUTTON CONTAINER */}
          <div style={{ minWidth: '160px', display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="primary" size="sm" onClick={handleOpenCreate}>
              <Plus className="w-4 h-4 mr-1.5" />
              {activeTab === 'teams' && (i18n.language === 'en' ? 'Add Team' : 'Yeni Ekip')}
              {activeTab === 'members' && (i18n.language === 'en' ? 'Add Person' : 'Yeni Kişi')}
              {activeTab === 'departments' && (i18n.language === 'en' ? 'Add Department' : 'Yeni Departman')}
            </Button>
          </div>
        </div>

        {/* TAB CONTENTS */}
        {activeTab === 'teams' && (
          <TeamsTab
            teams={teams}
            isLoading={isTeamsLoading}
            isError={isTeamsError}
            refetch={refetchTeams}
            onEdit={handleOpenEdit}
            onDelete={(tItem) => setDeleteConfirmItem({ type: 'teams', id: tItem.id, name: tItem.name })}
          />
        )}

        {activeTab === 'members' && (
          <MembersTab
            members={members}
            isLoading={isMembersLoading}
            isError={isMembersError}
            refetch={refetchMembers}
            onEdit={handleOpenEdit}
            onDelete={(m) => setDeleteConfirmItem({ type: 'members', id: m.id, name: `${m.firstName} ${m.lastName}` })}
          />
        )}

        {activeTab === 'departments' && (
          <DepartmentsTab
            departments={departments}
            isLoading={isDeptsLoading}
            isError={isDeptsError}
            refetch={refetchDepts}
            onEdit={handleOpenEdit}
            onDelete={(d) => setDeleteConfirmItem({ type: 'departments', id: d.id, name: d.name })}
          />
        )}

        {/* CREATE / EDIT MODAL */}
        {modalMode && (
          <OrganizationModal
            mode={modalMode}
            tab={activeTab}
            editingItem={editingItem}
            departments={departments || []}
            teams={teams || []}
            onClose={handleCloseModal}
            onSuccess={(msg) => {
              handleCloseModal();
              showToast('success', msg);
            }}
            onError={(msg) => showToast('error', msg)}
            createDeptMut={createDeptMut}
            updateDeptMut={updateDeptMut}
            createTeamMut={createTeamMut}
            updateTeamMut={updateTeamMut}
            createMemberMut={createMemberMut}
            updateMemberMut={updateMemberMut}
            updateMemberProjectAccessMut={updateMemberProjectAccessMut}
            createMemberAccountMut={createMemberAccountMut}
            linkMemberAccountMut={linkMemberAccountMut}
            updateMemberAdminRoleMut={updateMemberAdminRoleMut}
          />
        )}

        {/* DELETE CONFIRMATION MODAL */}
        {deleteConfirmItem && (
          <ConfirmDeleteModal
            itemName={deleteConfirmItem.name}
            itemType={deleteConfirmItem.type}
            onConfirm={handleDelete}
            onCancel={() => setDeleteConfirmItem(null)}
          />
        )}
      </div>
    </PageLayout>
  );
}

// ----------------------------------------------------------------------
// TAB BUTTON COMPONENT (STABLE GEOMETRY)
// ----------------------------------------------------------------------
function TabButton({
  active,
  onClick,
  icon,
  label,
  count,
}: {
  active: boolean;
  onClick: () => void;
  icon: React.ReactNode;
  label: string;
  count?: number;
}) {
  return (
    <button
      role="tab"
      aria-selected={active}
      onClick={onClick}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 'var(--space-2)',
        padding: 'var(--space-2) var(--space-4)',
        borderRadius: 'var(--radius-sm)',
        border: 'none',
        fontSize: 'var(--font-size-sm)',
        fontWeight: active ? 600 : 500,
        cursor: 'pointer',
        backgroundColor: active ? 'var(--color-surface)' : 'transparent',
        color: active ? 'var(--color-primary)' : 'var(--color-text-muted)',
        boxShadow: active ? '0 1px 2px rgba(0, 0, 0, 0.1)' : 'none',
        transition: 'all 0.15s ease-in-out',
        minWidth: '130px',
      }}
    >
      <span style={{ display: 'inline-flex', alignItems: 'center' }}>{icon}</span>
      <span>{label}</span>
      <span
        style={{
          minWidth: '22px',
          height: '20px',
          borderRadius: '10px',
          fontSize: '11px',
          fontWeight: 700,
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: active ? 'rgba(59, 130, 246, 0.15)' : 'var(--color-border)',
          color: active ? 'var(--color-primary)' : 'var(--color-text-muted)',
          padding: '0 5px',
        }}
      >
        {count ?? 0}
      </span>
    </button>
  );
}

// ----------------------------------------------------------------------
// TEAMS TAB
// ----------------------------------------------------------------------
function TeamsTab({
  teams,
  isLoading,
  isError,
  refetch,
  onEdit,
  onDelete,
}: {
  teams?: TeamAdmin[];
  isLoading: boolean;
  isError: boolean;
  refetch: () => void;
  onEdit: (team: TeamAdmin) => void;
  onDelete: (team: TeamAdmin) => void;
}) {
  const { t, i18n } = useTranslation(['organization', 'common']);
  if (isLoading) return <Skeleton height="260px" />;
  if (isError) return <ErrorState title={t('teams.loadErrorTitle', { ns: 'organization' })} description={t('teams.loadErrorDesc', { ns: 'organization' })} onRetry={refetch} />;
  if (!teams || teams.length === 0) {
    return <EmptyState icon={<Users className="w-10 h-10 text-slate-400" />} title={i18n.language === 'en' ? 'No Teams Defined Yet' : 'Henüz Ekip Tanımlanmamış'} description={i18n.language === 'en' ? 'Click Add Team to create the first team.' : 'Yeni Ekip butonunu kullanarak ilk ekibi ekleyebilirsiniz.'} />;
  }

  return (
    <Card style={{ padding: 0, overflow: 'hidden' }}>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: 'var(--font-size-sm)' }}>
          <thead>
            <tr style={{ backgroundColor: 'var(--color-bg)', borderBottom: '1px solid var(--color-border)' }}>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Team Name' : 'Ekip Adı'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Department' : 'Departman'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Description' : 'Açıklama'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'center' }}>{i18n.language === 'en' ? 'Projects' : 'Proje Sayısı'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'right' }}>{t('actions.edit', { ns: 'common' })}</th>
            </tr>
          </thead>
          <tbody>
            {teams.map((tItem) => (
              <tr key={tItem.id} style={{ borderBottom: '1px solid var(--color-border)' }}>
                <td style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text)' }}>
                  {tItem.name}
                  {tItem.code && <span style={{ fontSize: '11px', color: 'var(--color-text-disabled)', marginLeft: '6px' }}>({tItem.code})</span>}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text)', fontWeight: 500 }}>
                  {tItem.departmentName || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text-muted)', maxWidth: '300px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {tItem.description || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'center' }}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', fontWeight: 600, color: 'var(--color-text)' }}>
                    <FolderGit2 className="w-4 h-4 text-emerald-500" />
                    {tItem.projectCount ?? 0}
                  </span>
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'right' }}>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <button
                      onClick={() => onEdit(tItem)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid var(--color-border)', backgroundColor: 'var(--color-bg)', cursor: 'pointer', color: 'var(--color-text)' }}
                      title={t('actions.edit', { ns: 'common' })}
                    >
                      <Edit2 className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => onDelete(tItem)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid rgba(239, 68, 68, 0.3)', backgroundColor: 'rgba(239, 68, 68, 0.08)', cursor: 'pointer', color: '#dc2626' }}
                      title={t('actions.delete', { ns: 'common' })}
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}

// ----------------------------------------------------------------------
// MEMBERS TAB
// ----------------------------------------------------------------------
function MembersTab({
  members,
  isLoading,
  isError,
  refetch,
  onEdit,
  onDelete,
}: {
  members?: MemberAdmin[];
  isLoading: boolean;
  isError: boolean;
  refetch: () => void;
  onEdit: (member: MemberAdmin) => void;
  onDelete: (member: MemberAdmin) => void;
}) {
  const { t, i18n } = useTranslation(['organization', 'common']);
  if (isLoading) return <Skeleton height="260px" />;
  if (isError) return <ErrorState title={i18n.language === 'en' ? 'Failed to load members' : 'Kişiler Yüklenemedi'} description={t('errors.genericDesc', { ns: 'common' })} onRetry={refetch} />;
  if (!members || members.length === 0) {
    return <EmptyState icon={<UserCheck className="w-10 h-10 text-slate-400" />} title={i18n.language === 'en' ? 'No Members Defined Yet' : 'Henüz Kişi Tanımlanmamış'} description={i18n.language === 'en' ? 'Click Add Person to create the first member.' : 'Yeni Kişi butonunu kullanarak ilk üyeyi ekleyebilirsiniz.'} />;
  }

  return (
    <Card style={{ padding: 0, overflow: 'hidden' }}>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: 'var(--font-size-sm)' }}>
          <thead>
            <tr style={{ backgroundColor: 'var(--color-bg)', borderBottom: '1px solid var(--color-border)' }}>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{t('admin.members.columns.name', { ns: 'organization' })}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{t('admin.members.columns.title', { ns: 'organization' })}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Department / Team' : 'Departman / Ekip'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{t('admin.members.columns.email', { ns: 'organization' })}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'center' }}>{i18n.language === 'en' ? 'Project Roles' : 'Proje Katılımı'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'center' }}>{t('admin.access.canCreateProjects', { ns: 'organization' })}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'center' }}>{i18n.language === 'en' ? 'Admin Role' : 'Yönetici Rolü'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'right' }}>{t('admin.members.columns.actions', { ns: 'organization' })}</th>
            </tr>
          </thead>
          <tbody>
            {members.map((m) => (
              <tr key={m.id} style={{ borderBottom: '1px solid var(--color-border)' }}>
                <td style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text)' }}>
                  {m.firstName} {m.lastName}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text-muted)' }}>
                  {m.title || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text)' }}>
                  {m.departmentName || m.teamName ? (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '2px', fontSize: '12px' }}>
                      {m.departmentName && (
                        <span style={{ fontWeight: 600, color: 'var(--color-primary)' }}>{m.departmentName}</span>
                      )}
                      {m.teamName && (
                        <span style={{ color: 'var(--color-text-muted)' }}>› {m.teamName}</span>
                      )}
                    </div>
                  ) : (
                    <span style={{ color: 'var(--color-text-muted)' }}>—</span>
                  )}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text-muted)' }}>
                  {m.email || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'center' }}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', fontWeight: 600, color: 'var(--color-text)' }}>
                    <FolderGit2 className="w-4 h-4 text-emerald-500" />
                    {m.projectCount ?? 0} {i18n.language === 'en' ? 'projects' : 'proje'}
                  </span>
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'center' }}>
                  {!m.hasApplicationAccount ? (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(148, 163, 184, 0.1)',
                        color: 'var(--color-text-muted)',
                        border: '1px solid rgba(148, 163, 184, 0.25)',
                      }}
                    >
                      {t('admin.members.noAccount', { ns: 'organization' })}
                    </span>
                  ) : m.applicationUserIsActive === false ? (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(245, 158, 11, 0.12)',
                        color: '#f59e0b',
                        border: '1px solid rgba(245, 158, 11, 0.3)',
                      }}
                    >
                      {i18n.language === 'en' ? 'Inactive Account' : 'Pasif Hesap'}
                    </span>
                  ) : m.canCreateProjects ? (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: '4px',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(34, 197, 94, 0.12)',
                        color: '#16a34a',
                        border: '1px solid rgba(34, 197, 94, 0.3)',
                      }}
                    >
                      <CheckCircle2 className="w-3 h-3" />
                      {i18n.language === 'en' ? 'Authorized' : 'Yetkili'}
                    </span>
                  ) : (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(148, 163, 184, 0.08)',
                        color: 'var(--color-text-muted)',
                        border: '1px solid var(--color-border)',
                      }}
                    >
                      {i18n.language === 'en' ? 'Standard' : 'Yetkisiz'}
                    </span>
                  )}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'center' }}>
                  {m.isSuperAdmin ? (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: '4px',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(124, 58, 237, 0.15)',
                        color: '#c084fc',
                        border: '1px solid rgba(124, 58, 237, 0.3)',
                      }}
                    >
                      <Shield className="w-3 h-3" />
                      SuperAdmin
                    </span>
                  ) : m.isAdmin ? (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: '4px',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(2, 132, 199, 0.15)',
                        color: '#38bdf8',
                        border: '1px solid rgba(2, 132, 199, 0.3)',
                      }}
                    >
                      <Shield className="w-3 h-3" />
                      Admin
                    </span>
                  ) : (
                    <span
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        padding: '2px 8px',
                        borderRadius: '9999px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor: 'rgba(148, 163, 184, 0.08)',
                        color: 'var(--color-text-muted)',
                        border: '1px solid var(--color-border)',
                      }}
                    >
                      {i18n.language === 'en' ? 'User' : 'Standart'}
                    </span>
                  )}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'right' }}>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <button
                      onClick={() => onEdit(m)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid var(--color-border)', backgroundColor: 'var(--color-bg)', cursor: 'pointer', color: 'var(--color-text)' }}
                      title={t('actions.edit', { ns: 'common' })}
                    >
                      <Edit2 className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => onDelete(m)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid rgba(239, 68, 68, 0.3)', backgroundColor: 'rgba(239, 68, 68, 0.08)', cursor: 'pointer', color: '#dc2626' }}
                      title={t('actions.delete', { ns: 'common' })}
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}

// ----------------------------------------------------------------------
// DEPARTMENTS TAB
// ----------------------------------------------------------------------
function DepartmentsTab({
  departments,
  isLoading,
  isError,
  refetch,
  onEdit,
  onDelete,
}: {
  departments?: DepartmentAdmin[];
  isLoading: boolean;
  isError: boolean;
  refetch: () => void;
  onEdit: (dept: DepartmentAdmin) => void;
  onDelete: (dept: DepartmentAdmin) => void;
}) {
  const { t, i18n } = useTranslation(['common']);
  if (isLoading) return <Skeleton height="260px" />;
  if (isError) return <ErrorState title={i18n.language === 'en' ? 'Failed to load departments' : 'Departmanlar Yüklenemedi'} description={t('errors.genericDesc', { ns: 'common' })} onRetry={refetch} />;
  if (!departments || departments.length === 0) {
    return <EmptyState icon={<Building2 className="w-10 h-10 text-slate-400" />} title={i18n.language === 'en' ? 'No Departments Defined Yet' : 'Henüz Departman Tanımlanmamış'} description={i18n.language === 'en' ? 'Click Add Department to create the first department.' : 'Yeni Departman butonunu kullanarak ilk departmanı ekleyebilirsiniz.'} />;
  }

  return (
    <Card style={{ padding: 0, overflow: 'hidden' }}>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: 'var(--font-size-sm)' }}>
          <thead>
            <tr style={{ backgroundColor: 'var(--color-bg)', borderBottom: '1px solid var(--color-border)' }}>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Department Name' : 'Departman Adı'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Code' : 'Kod'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)' }}>{i18n.language === 'en' ? 'Description' : 'Açıklama'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'center' }}>{i18n.language === 'en' ? 'Teams' : 'Ekip Sayısı'}</th>
              <th style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text-muted)', textAlign: 'right' }}>{t('actions.edit', { ns: 'common' })}</th>
            </tr>
          </thead>
          <tbody>
            {departments.map((d) => (
              <tr key={d.id} style={{ borderBottom: '1px solid var(--color-border)' }}>
                <td style={{ padding: 'var(--space-3) var(--space-4)', fontWeight: 600, color: 'var(--color-text)' }}>
                  {d.name}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text-muted)' }}>
                  {d.code || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', color: 'var(--color-text-muted)', maxWidth: '300px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {d.description || '—'}
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'center' }}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', fontWeight: 600, color: 'var(--color-text)' }}>
                    <Users className="w-4 h-4 text-blue-500" />
                    {d.teamCount ?? 0}
                  </span>
                </td>
                <td style={{ padding: 'var(--space-3) var(--space-4)', textAlign: 'right' }}>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)' }}>
                    <button
                      onClick={() => onEdit(d)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid var(--color-border)', backgroundColor: 'var(--color-bg)', cursor: 'pointer', color: 'var(--color-text)' }}
                      title={t('actions.edit', { ns: 'common' })}
                    >
                      <Edit2 className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => onDelete(d)}
                      style={{ padding: '4px 8px', borderRadius: 'var(--radius-sm)', border: '1px solid rgba(239, 68, 68, 0.3)', backgroundColor: 'rgba(239, 68, 68, 0.08)', cursor: 'pointer', color: '#dc2626' }}
                      title={t('actions.delete', { ns: 'common' })}
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}

// ----------------------------------------------------------------------
// ORGANIZATION FORM MODAL (OPAQUE SURFACE)
// ----------------------------------------------------------------------
function OrganizationModal({
  mode,
  tab,
  editingItem,
  departments,
  teams,
  onClose,
  onSuccess,
  onError,
  createDeptMut,
  updateDeptMut,
  createTeamMut,
  updateTeamMut,
  createMemberMut,
  updateMemberMut,
  updateMemberProjectAccessMut,
  createMemberAccountMut,
  linkMemberAccountMut,
  updateMemberAdminRoleMut,
}: {
  mode: 'create' | 'edit';
  tab: TabType;
  editingItem: any;
  departments: DepartmentAdmin[];
  teams: TeamAdmin[];
  onClose: () => void;
  onSuccess: (msg: string) => void;
  onError: (msg: string) => void;
  createDeptMut: any;
  updateDeptMut: any;
  createTeamMut: any;
  updateTeamMut: any;
  createMemberMut: any;
  updateMemberMut: any;
  updateMemberProjectAccessMut: any;
  createMemberAccountMut: any;
  linkMemberAccountMut: any;
  updateMemberAdminRoleMut: any;
}) {
  const { t, i18n } = useTranslation(['organization', 'common']);
  const { user: currentAuthUser } = useAuth();
  const isSuperAdmin = !!currentAuthUser?.isSuperAdmin;

  // Form State
  const [name, setName] = useState(editingItem?.name || '');
  const [code, setCode] = useState(editingItem?.code || '');
  const [description, setDescription] = useState(editingItem?.description || '');
  const [departmentId, setDepartmentId] = useState<number | undefined>(editingItem?.departmentId || undefined);

  const [firstName, setFirstName] = useState(editingItem?.firstName || '');
  const [lastName, setLastName] = useState(editingItem?.lastName || '');
  const [title, setTitle] = useState(editingItem?.title || '');
  const [email, setEmail] = useState(editingItem?.email || '');
  const [memberDepartmentId, setMemberDepartmentId] = useState<number | undefined>(editingItem?.departmentId || undefined);
  const [memberTeamId, setMemberTeamId] = useState<number | undefined>(editingItem?.teamId || undefined);

  // Member Access & Account State
  const [canCreateProjectsState, setCanCreateProjectsState] = useState<boolean>(editingItem?.canCreateProjects || false);
  const [isAdminState, setIsAdminState] = useState<boolean>(editingItem?.isAdmin || false);
  const [showCreateAccountForm, setShowCreateAccountForm] = useState(false);
  const [newAccountPassword, setNewAccountPassword] = useState('');
  const [newAccountConfirmPassword, setNewAccountConfirmPassword] = useState('');
  const [isUpdatingAccess, setIsUpdatingAccess] = useState(false);
  const [isUpdatingAdminRole, setIsUpdatingAdminRole] = useState(false);
  const [showAdminRoleConfirmModal, setShowAdminRoleConfirmModal] = useState<boolean | null>(null);
  const [isCreatingAccount, setIsCreatingAccount] = useState(false);
  const [isLinkingAccount, setIsLinkingAccount] = useState(false);

  const [isSubmitting, setIsSubmitting] = useState(false);

  // Close on Escape
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && showAdminRoleConfirmModal === null) onClose();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onClose, showAdminRoleConfirmModal]);

  const handleToggleProjectAccess = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const nextVal = e.target.checked;
    setCanCreateProjectsState(nextVal);
    if (!editingItem?.id) return;
    setIsUpdatingAccess(true);
    try {
      await updateMemberProjectAccessMut.mutateAsync({
        memberId: editingItem.id,
        payload: { canCreateProjects: nextVal }
      });
      onSuccess(nextVal ? (i18n.language === 'en' ? 'Project creation permission granted.' : 'Proje ekleme ve düzenleme yetkisi verildi.') : (i18n.language === 'en' ? 'Project creation permission revoked.' : 'Proje ekleme ve düzenleme yetkisi kaldırıldı.'));
    } catch (err: any) {
      setCanCreateProjectsState(!nextVal); // revert
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'Failed to update access.' : 'Yetki güncellenemedi.');
      onError(msg);
    } finally {
      setIsUpdatingAccess(false);
    }
  };

  const handleCreateAccountSubmit = async (e: React.MouseEvent) => {
    e.preventDefault();
    if (!editingItem?.id) return;
    if (newAccountPassword.length < 6) {
      onError(i18n.language === 'en' ? 'Password must be at least 6 characters.' : 'Şifre en az 6 karakter olmalıdır.');
      return;
    }
    if (newAccountPassword !== newAccountConfirmPassword) {
      onError(i18n.language === 'en' ? 'Passwords do not match.' : 'Şifreler eşleşmiyor.');
      return;
    }
    setIsCreatingAccount(true);
    try {
      await createMemberAccountMut.mutateAsync({
        memberId: editingItem.id,
        payload: { password: newAccountPassword, confirmPassword: newAccountConfirmPassword }
      });
      onSuccess(i18n.language === 'en' ? `Application account created and linked for "${editingItem.firstName} ${editingItem.lastName}".` : `"${editingItem.firstName} ${editingItem.lastName}" için uygulama hesabı başarıyla oluşturuldu ve bağlandı.`);
      setShowCreateAccountForm(false);
      setNewAccountPassword('');
      setNewAccountConfirmPassword('');
      onClose();
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'Failed to create account.' : 'Hesap oluşturulamadı.');
      onError(msg);
    } finally {
      setIsCreatingAccount(false);
    }
  };

  const handleLinkExistingAccount = async (e: React.MouseEvent) => {
    e.preventDefault();
    if (!editingItem?.id) return;
    setIsLinkingAccount(true);
    try {
      await linkMemberAccountMut.mutateAsync({
        memberId: editingItem.id,
        payload: { userId: editingItem.matchingUnlinkedUserId }
      });
      onSuccess(i18n.language === 'en' ? `Existing application account linked for "${editingItem.firstName} ${editingItem.lastName}".` : `"${editingItem.firstName} ${editingItem.lastName}" için mevcut uygulama hesabı başarıyla bağlandı.`);
      onClose();
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'Failed to link account.' : 'Hesap bağlanamadı.');
      onError(msg);
    } finally {
      setIsLinkingAccount(false);
    }
  };

  const handleAdminRoleConfirm = async (newVal: boolean) => {
    if (!editingItem?.id) return;
    setIsUpdatingAdminRole(true);
    try {
      await updateMemberAdminRoleMut.mutateAsync({
        memberId: editingItem.id,
        payload: { isAdmin: newVal }
      });
      setIsAdminState(newVal);
      setShowAdminRoleConfirmModal(null);
      onSuccess(newVal
        ? (i18n.language === 'en' ? 'Administrator role granted.' : 'Yönetici (Admin) rolü başarıyla verildi.')
        : (i18n.language === 'en' ? 'Administrator role revoked.' : 'Yönetici (Admin) rolü başarıyla kaldırıldı.'));
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'Failed to update admin role.' : 'Yönetici rolü güncellenemedi.');
      onError(msg);
    } finally {
      setIsUpdatingAdminRole(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (tab === 'departments') {
        if (mode === 'create') {
          await createDeptMut.mutateAsync({ name, code: code || undefined, description: description || undefined });
          onSuccess(i18n.language === 'en' ? `Department "${name}" was created.` : `"${name}" departmanı oluşturuldu.`);
        } else {
          await updateDeptMut.mutateAsync({ id: editingItem.id, payload: { name, code: code || undefined, description: description || undefined } });
          onSuccess(i18n.language === 'en' ? `Department "${name}" was updated.` : `"${name}" departmanı güncellendi.`);
        }
      } else if (tab === 'teams') {
        if (mode === 'create') {
          await createTeamMut.mutateAsync({ name, code: code || undefined, departmentId: departmentId ? Number(departmentId) : undefined, description: description || undefined });
          onSuccess(i18n.language === 'en' ? `Team "${name}" was created.` : `"${name}" ekibi oluşturuldu.`);
        } else {
          await updateTeamMut.mutateAsync({ id: editingItem.id, payload: { name, code: code || undefined, departmentId: departmentId ? Number(departmentId) : undefined, description: description || undefined } });
          onSuccess(i18n.language === 'en' ? `Team "${name}" was updated.` : `"${name}" ekibi güncellendi.`);
        }
      } else if (tab === 'members') {
        if (mode === 'create') {
          await createMemberMut.mutateAsync({
            firstName,
            lastName,
            title: title || undefined,
            email: email || undefined,
            departmentId: memberDepartmentId ? Number(memberDepartmentId) : undefined,
            teamId: memberTeamId ? Number(memberTeamId) : undefined,
          });
          onSuccess(i18n.language === 'en' ? `Member "${firstName} ${lastName}" was added.` : `"${firstName} ${lastName}" üyesi eklendi.`);
        } else {
          await updateMemberMut.mutateAsync({
            id: editingItem.id,
            payload: {
              firstName,
              lastName,
              title: title || undefined,
              email: email || undefined,
              departmentId: memberDepartmentId ? Number(memberDepartmentId) : undefined,
              teamId: memberTeamId ? Number(memberTeamId) : undefined,
            }
          });
          onSuccess(i18n.language === 'en' ? `Member "${firstName} ${lastName}" was updated.` : `"${firstName} ${lastName}" üyesi güncellendi.`);
        }
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || (i18n.language === 'en' ? 'Operation failed.' : 'İşlem başarısız oldu.');
      onError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const getTitleText = () => {
    const action = mode === 'create' ? (i18n.language === 'en' ? 'Add' : 'Yeni') : (i18n.language === 'en' ? 'Edit:' : 'Düzenle:');
    if (tab === 'teams') return `${action} ${i18n.language === 'en' ? 'Team' : 'Ekip'} ${mode === 'edit' ? editingItem?.name : ''}`;
    if (tab === 'members') return `${action} ${i18n.language === 'en' ? 'Person' : 'Kişi'} ${mode === 'edit' ? `${editingItem?.firstName} ${editingItem?.lastName}` : ''}`;
    return `${action} ${i18n.language === 'en' ? 'Department' : 'Departman'} ${mode === 'edit' ? editingItem?.name : ''}`;
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.65)',
        backdropFilter: 'blur(3px)',
        padding: 'var(--space-4)',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '540px',
          backgroundColor: '#1e293b',
          color: '#f8fafc',
          borderRadius: 'var(--radius-lg)',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.3)',
          border: '1px solid #334155',
          overflow: 'hidden',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {/* MODAL HEADER */}
        <div
          style={{
            padding: 'var(--space-4) var(--space-5)',
            borderBottom: '1px solid #334155',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            backgroundColor: '#0f172a',
          }}
        >
          <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, color: '#f8fafc', margin: 0 }}>
            {getTitleText()}
          </h3>
          <button
            onClick={onClose}
            style={{ background: 'none', border: 'none', color: '#94a3b8', cursor: 'pointer', padding: '4px' }}
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* MODAL FORM */}
        <form onSubmit={handleSubmit} style={{ padding: 'var(--space-5)', display: 'flex', flexDirection: 'column', gap: 'var(--space-4)', overflowY: 'auto' }}>
          {tab === 'teams' && (
            <>
              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Team Name *' : 'Ekip Adı *'}
                </label>
                <input
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder={i18n.language === 'en' ? 'e.g. Software Development Team' : 'örn. Yazılım Geliştirme Ekibi'}
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Department' : 'Departman'}
                </label>
                <select
                  value={departmentId || ''}
                  onChange={(e) => setDepartmentId(e.target.value ? Number(e.target.value) : undefined)}
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                >
                  <option value="">{i18n.language === 'en' ? '-- Select Department --' : '-- Departman Seçiniz --'}</option>
                  {departments.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Team Code' : 'Ekip Kodu'}
                </label>
                <input
                  type="text"
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  placeholder="örn. DEV-TEAM"
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Description' : 'Açıklama'}
                </label>
                <textarea
                  rows={3}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder={i18n.language === 'en' ? 'Organizational role and domain...' : 'Ekibin kurumsal tanımı ve görev alanı...'}
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px', resize: 'vertical' }}
                />
              </div>
            </>
          )}

          {tab === 'members' && (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
                <div>
                  <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                    {i18n.language === 'en' ? 'First Name *' : 'Ad *'}
                  </label>
                  <input
                    type="text"
                    required
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="örn. Ahmet"
                    style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                    {i18n.language === 'en' ? 'Last Name *' : 'Soyad *'}
                  </label>
                  <input
                    type="text"
                    required
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="örn. Yılmaz"
                    style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                  />
                </div>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {t('admin.members.columns.title', { ns: 'organization' })}
                </label>
                <input
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="örn. Kıdemli Yazılım Mimarı"
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {t('admin.members.columns.email', { ns: 'organization' })}
                </label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="ahmet.yilmaz@demirexport.com"
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              {/* Cascading Department and Team Selectors */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
                <div>
                  <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                    {i18n.language === 'en' ? 'Department' : 'Departman'}
                  </label>
                  <select
                    value={memberDepartmentId || ''}
                    onChange={(e) => {
                      const nextDeptId = e.target.value ? Number(e.target.value) : undefined;
                      setMemberDepartmentId(nextDeptId);
                      if (memberTeamId) {
                        const currentTeam = teams.find(t => t.id === memberTeamId);
                        if (currentTeam && currentTeam.departmentId !== nextDeptId) {
                          setMemberTeamId(undefined);
                        }
                      }
                    }}
                    style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                  >
                    <option value="">{i18n.language === 'en' ? '-- Select Department --' : '-- Departman Seçiniz --'}</option>
                    {departments.map((d) => (
                      <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                    {i18n.language === 'en' ? 'Team' : 'Ekip'}
                  </label>
                  <select
                    value={memberTeamId || ''}
                    onChange={(e) => {
                      const nextTeamId = e.target.value ? Number(e.target.value) : undefined;
                      setMemberTeamId(nextTeamId);
                      if (nextTeamId) {
                        const selectedTeam = teams.find(t => t.id === nextTeamId);
                        if (selectedTeam?.departmentId) {
                          setMemberDepartmentId(selectedTeam.departmentId);
                        }
                      }
                    }}
                    style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                  >
                    <option value="">{i18n.language === 'en' ? '-- Select Team --' : '-- Ekip Seçiniz --'}</option>
                    {teams
                      .filter(t => !memberDepartmentId || t.departmentId === memberDepartmentId)
                      .map((t) => (
                        <option key={t.id} value={t.id}>
                          {t.name} {t.departmentName && !memberDepartmentId ? `(${t.departmentName})` : ''}
                        </option>
                      ))}
                  </select>
                </div>
              </div>

              {/* ─── UYGULAMA ERİŞİMİ ────────────────────────────── */}
              <div style={{ marginTop: 'var(--space-2)', paddingTop: 'var(--space-4)', borderTop: '1px solid #334155' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '10px' }}>
                  <Shield className="w-4 h-4 text-emerald-400" />
                  <span style={{ fontSize: '12px', fontWeight: 700, color: '#e2e8f0', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    {i18n.language === 'en' ? 'Application Access' : 'Uygulama Erişimi'}
                  </span>
                </div>

                {mode === 'create' ? (
                  <div style={{ padding: '10px 12px', borderRadius: '6px', backgroundColor: '#0f172a', border: '1px solid #334155', color: '#94a3b8', fontSize: '12px' }}>
                    {i18n.language === 'en' ? 'After creating the member, you can link or create an application account from the Edit screen.' : "Kişi oluşturulduktan sonra 'Düzenle' ekranından uygulama hesabı oluşturulabilir veya bağlanabilir."}
                  </div>
                ) : editingItem?.hasApplicationAccount ? (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', padding: '12px', borderRadius: '6px', backgroundColor: '#0f172a', border: '1px solid #334155' }}>
                    {/* Account Status */}
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '6px' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px' }}>
                        <span style={{ color: '#94a3b8' }}>{i18n.language === 'en' ? 'Login Account:' : 'Giriş Hesabı:'}</span>
                        <span style={{ fontWeight: 600, color: '#f8fafc' }}>
                          {editingItem.applicationUserEmail || editingItem.email}
                        </span>
                      </div>
                      <span
                        style={{
                          fontSize: '11px',
                          fontWeight: 600,
                          padding: '2px 8px',
                          borderRadius: '4px',
                          backgroundColor: editingItem.applicationUserIsActive !== false ? 'rgba(34, 197, 94, 0.15)' : 'rgba(245, 158, 11, 0.15)',
                          color: editingItem.applicationUserIsActive !== false ? '#4ade80' : '#fbbf24',
                          border: `1px solid ${editingItem.applicationUserIsActive !== false ? 'rgba(34, 197, 94, 0.3)' : 'rgba(245, 158, 11, 0.3)'}`,
                        }}
                      >
                        {editingItem.applicationUserIsActive !== false ? (i18n.language === 'en' ? 'Active Account' : 'Aktif Hesap') : (i18n.language === 'en' ? 'Inactive Account' : 'Pasif Hesap')}
                      </span>
                    </div>

                    {/* Email mismatch subtle warning */}
                    {editingItem.email && editingItem.applicationUserEmail && editingItem.email.trim().toLowerCase() !== editingItem.applicationUserEmail.trim().toLowerCase() && (
                      <div style={{ fontSize: '12px', color: '#fbbf24', backgroundColor: 'rgba(245, 158, 11, 0.1)', padding: '6px 10px', borderRadius: '4px', border: '1px solid rgba(245, 158, 11, 0.2)' }}>
                        ⚠️ {i18n.language === 'en' ? 'Organization email and login account email differ.' : 'Organizasyon e-postası ile giriş hesabı e-postası farklı.'}
                      </div>
                    )}

                    {/* Project Creation Permission Checkbox */}
                    <div style={{ paddingTop: '8px', borderTop: '1px solid #1e293b' }}>
                      <label style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: editingItem.applicationUserIsActive !== false ? 'pointer' : 'not-allowed' }}>
                        <input
                          type="checkbox"
                          checked={canCreateProjectsState}
                          disabled={editingItem.applicationUserIsActive === false || isUpdatingAccess}
                          onChange={handleToggleProjectAccess}
                          style={{ marginTop: '3px', width: '16px', height: '16px', accentColor: '#10b981', cursor: 'inherit' }}
                        />
                        <div>
                          <div style={{ fontSize: '13px', fontWeight: 600, color: '#f8fafc' }}>
                            {t('admin.access.canCreateProjects', { ns: 'organization' })}
                          </div>
                          <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px', lineHeight: 1.4 }}>
                            {t('admin.access.grantedDesc', { ns: 'organization' })}
                          </div>
                          {editingItem.applicationUserIsActive === false && (
                            <div style={{ fontSize: '11px', color: '#fbbf24', marginTop: '4px' }}>
                              * {i18n.language === 'en' ? 'Inactive accounts cannot use this permission.' : 'Pasif hesaplar bu yetkiyi kullanamaz.'}
                            </div>
                          )}
                        </div>
                      </label>
                    </div>

                    {/* Admin Role Section */}
                    <div style={{ paddingTop: '8px', borderTop: '1px solid #1e293b' }}>
                      {isSuperAdmin ? (
                        <div>
                          <label style={{ display: 'flex', alignItems: 'flex-start', gap: '10px', cursor: editingItem.isSuperAdmin ? 'not-allowed' : 'pointer' }}>
                            <input
                              type="checkbox"
                              checked={editingItem.isSuperAdmin || isAdminState}
                              disabled={editingItem.isSuperAdmin || isUpdatingAdminRole}
                              onChange={(e) => {
                                setShowAdminRoleConfirmModal(e.target.checked);
                              }}
                              style={{ marginTop: '3px', width: '16px', height: '16px', accentColor: '#38bdf8', cursor: 'inherit' }}
                            />
                            <div>
                              <div style={{ fontSize: '13px', fontWeight: 600, color: '#f8fafc', display: 'flex', alignItems: 'center', gap: '6px' }}>
                                <Shield className="w-4 h-4 text-sky-400" />
                                {i18n.language === 'en' ? 'Administrator (Admin) Role' : 'Yönetici (Admin) Rolü'}
                                {editingItem.isSuperAdmin && (
                                  <span style={{ fontSize: '11px', color: '#c084fc', backgroundColor: 'rgba(124, 58, 237, 0.2)', padding: '1px 6px', borderRadius: '4px' }}>
                                    SuperAdmin
                                  </span>
                                )}
                              </div>
                              <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px', lineHeight: 1.4 }}>
                                {editingItem.isSuperAdmin
                                  ? (i18n.language === 'en' ? 'SuperAdmin account has highest authority.' : 'Süper Yönetici hesabı en üst düzey yetkiye sahiptir.')
                                  : (i18n.language === 'en' ? 'Grants full administrative privileges including reviewing, approving, and managing all projects.' : 'Tüm projeleri görüntüleme, onaylama, düzenleme ve yönetim paneline tam erişim yetkisi verir.')}
                              </div>
                            </div>
                          </label>
                        </div>
                      ) : (
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '4px 0' }}>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px' }}>
                            <Shield className="w-4 h-4 text-slate-400" />
                            <span style={{ color: '#94a3b8' }}>{i18n.language === 'en' ? 'Admin Role:' : 'Yönetici Rolü:'}</span>
                            <span style={{ fontWeight: 600, color: editingItem.isAdmin ? '#38bdf8' : '#cbd5e1' }}>
                              {editingItem.isSuperAdmin ? 'SuperAdmin' : editingItem.isAdmin ? 'Admin' : (i18n.language === 'en' ? 'Standard User' : 'Standart Kullanıcı')}
                            </span>
                          </div>
                          <span style={{ fontSize: '11px', color: '#94a3b8', fontStyle: 'italic' }}>
                            {i18n.language === 'en' ? 'SuperAdmin only' : 'Yalnızca SuperAdmin değiştirebilir'}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', padding: '12px', borderRadius: '6px', backgroundColor: '#0f172a', border: '1px solid #334155' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <span style={{ fontSize: '13px', color: '#94a3b8' }}>
                        {t('admin.members.noAccount', { ns: 'organization' })}
                      </span>
                      <span
                        style={{
                          fontSize: '11px',
                          fontWeight: 600,
                          padding: '2px 8px',
                          borderRadius: '4px',
                          backgroundColor: 'rgba(148, 163, 184, 0.1)',
                          color: '#94a3b8',
                          border: '1px solid rgba(148, 163, 184, 0.2)',
                        }}
                      >
                        {t('admin.members.noAccount', { ns: 'organization' })}
                      </span>
                    </div>

                    {/* Matching unlinked account banner */}
                    {editingItem?.hasMatchingUnlinkedAccount && !showCreateAccountForm && (
                      <div style={{ padding: '8px 10px', borderRadius: '6px', backgroundColor: 'rgba(59, 130, 246, 0.1)', border: '1px solid rgba(59, 130, 246, 0.3)', display: 'flex', flexDirection: 'column', gap: '6px' }}>
                        <div style={{ fontSize: '12px', color: '#93c5fd' }}>
                          ℹ️ {i18n.language === 'en' ? 'An existing account was found matching this email.' : 'Bu e-posta adresiyle sistemde mevcut bir uygulama hesabı bulundu.'}
                        </div>
                        <Button
                          type="button"
                          variant="secondary"
                          size="sm"
                          disabled={isLinkingAccount}
                          onClick={handleLinkExistingAccount}
                          style={{ alignSelf: 'flex-start', borderColor: '#3b82f6', color: '#93c5fd' }}
                        >
                          {isLinkingAccount ? (i18n.language === 'en' ? 'Linking...' : 'Bağlanıyor...') : (i18n.language === 'en' ? 'Link Existing Account' : 'Mevcut Hesabı Bağla')}
                        </Button>
                      </div>
                    )}

                    {/* Create Account Action or Subform */}
                    {!showCreateAccountForm ? (
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() => setShowCreateAccountForm(true)}
                        style={{ alignSelf: 'flex-start', borderColor: '#475569', color: '#cbd5e1' }}
                      >
                        <KeyRound className="w-3.5 h-3.5 mr-1 text-emerald-400" />
                        {i18n.language === 'en' ? 'Create Application Account' : 'Uygulama Hesabı Oluştur'}
                      </Button>
                    ) : (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', paddingTop: '8px', borderTop: '1px solid #1e293b' }}>
                        <div style={{ fontSize: '12px', fontWeight: 600, color: '#f8fafc' }}>
                          {i18n.language === 'en' ? 'Create Login Account' : 'Yeni Giriş Hesabı Oluştur'}
                        </div>
                        <div>
                          <label style={{ display: 'block', fontSize: '11px', color: '#94a3b8', marginBottom: '3px' }}>
                            {i18n.language === 'en' ? 'Account Email' : 'Hesap E-postası'}
                          </label>
                          <input
                            type="text"
                            disabled
                            value={email || editingItem?.email || ''}
                            style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #334155', backgroundColor: '#1e293b', color: '#94a3b8', fontSize: '13px' }}
                          />
                        </div>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
                          <div>
                            <label style={{ display: 'block', fontSize: '11px', color: '#cbd5e1', marginBottom: '3px' }}>
                              {i18n.language === 'en' ? 'Initial Password *' : 'Başlangıç Şifresi *'}
                            </label>
                            <input
                              type="password"
                              value={newAccountPassword}
                              onChange={(e) => setNewAccountPassword(e.target.value)}
                              placeholder={i18n.language === 'en' ? 'Min 6 chars' : 'En az 6 karakter'}
                              style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #475569', backgroundColor: '#1e293b', color: '#f8fafc', fontSize: '13px' }}
                            />
                          </div>
                          <div>
                            <label style={{ display: 'block', fontSize: '11px', color: '#cbd5e1', marginBottom: '3px' }}>
                              {i18n.language === 'en' ? 'Confirm Password *' : 'Şifre Tekrarı *'}
                            </label>
                            <input
                              type="password"
                              value={newAccountConfirmPassword}
                              onChange={(e) => setNewAccountConfirmPassword(e.target.value)}
                              placeholder={i18n.language === 'en' ? 'Re-enter password' : 'Şifreyi tekrar girin'}
                              style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #475569', backgroundColor: '#1e293b', color: '#f8fafc', fontSize: '13px' }}
                            />
                          </div>
                        </div>
                        <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end', marginTop: '4px' }}>
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => {
                              setShowCreateAccountForm(false);
                              setNewAccountPassword('');
                              setNewAccountConfirmPassword('');
                            }}
                            disabled={isCreatingAccount}
                          >
                            {t('actions.cancel', { ns: 'common' })}
                          </Button>
                          <Button
                            type="button"
                            variant="primary"
                            size="sm"
                            disabled={isCreatingAccount || !newAccountPassword || !newAccountConfirmPassword}
                            onClick={handleCreateAccountSubmit}
                          >
                            {isCreatingAccount ? (i18n.language === 'en' ? 'Creating...' : 'Oluşturuluyor...') : (i18n.language === 'en' ? 'Create & Link Account' : 'Hesap Oluştur ve Bağla')}
                          </Button>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </div>
            </>
          )}

          {tab === 'departments' && (
            <>
              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Department Name *' : 'Departman Adı *'}
                </label>
                <input
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder={i18n.language === 'en' ? 'e.g. Digital Transformation Directorate' : 'örn. Dijital Dönüşüm Direktörlüğü'}
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Department Code' : 'Departman Kodu'}
                </label>
                <input
                  type="text"
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  placeholder="örn. DDD-01"
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 'var(--font-size-xs)', fontWeight: 600, marginBottom: '6px', color: '#cbd5e1' }}>
                  {i18n.language === 'en' ? 'Description' : 'Açıklama'}
                </label>
                <textarea
                  rows={3}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder={i18n.language === 'en' ? 'Department organizational scope...' : 'Departmanın organizasyonel görevi...'}
                  style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #475569', backgroundColor: '#0f172a', color: '#f8fafc', fontSize: '14px', resize: 'vertical' }}
                />
              </div>
            </>
          )}

          {/* FOOTER BUTTONS */}
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)', marginTop: 'var(--space-3)', paddingTop: 'var(--space-4)', borderTop: '1px solid #334155' }}>
            <Button type="button" variant="secondary" onClick={onClose} disabled={isSubmitting} style={{ borderColor: '#475569', color: '#cbd5e1' }}>
              {t('actions.cancel', { ns: 'common' })}
            </Button>
            <Button type="submit" variant="primary" disabled={isSubmitting}>
              {isSubmitting ? (i18n.language === 'en' ? 'Saving...' : 'Kaydediliyor...') : mode === 'create' ? (i18n.language === 'en' ? 'Add' : 'Ekle') : (i18n.language === 'en' ? 'Update' : 'Güncelle')}
            </Button>
          </div>
        </form>
      </div>

      {/* Admin Role Confirmation Modal */}
      {showAdminRoleConfirmModal !== null && (
        <div
          style={{
            position: 'fixed',
            inset: 0,
            zIndex: 1100,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: 'rgba(0, 0, 0, 0.75)',
            padding: 'var(--space-4)',
          }}
        >
          <div
            style={{
              width: '100%',
              maxWidth: '440px',
              backgroundColor: '#0f172a',
              color: '#f8fafc',
              borderRadius: 'var(--radius-lg)',
              boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.7)',
              border: '1px solid #334155',
              padding: 'var(--space-5)',
              display: 'flex',
              flexDirection: 'column',
              gap: 'var(--space-4)',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
              <div
                style={{
                  padding: '8px',
                  borderRadius: '50%',
                  backgroundColor: showAdminRoleConfirmModal ? 'rgba(56, 189, 248, 0.15)' : 'rgba(239, 68, 68, 0.15)',
                  color: showAdminRoleConfirmModal ? '#38bdf8' : '#ef4444',
                }}
              >
                <Shield className="w-6 h-6" />
              </div>
              <div>
                <h4 style={{ margin: 0, fontSize: '15px', fontWeight: 600, color: '#f8fafc' }}>
                  {showAdminRoleConfirmModal
                    ? (i18n.language === 'en' ? 'Grant Admin Role?' : 'Yönetici (Admin) Yetkisi Verilsin mi?')
                    : (i18n.language === 'en' ? 'Revoke Admin Role?' : 'Yönetici (Admin) Yetkisi Kaldırılsın mı?')}
                </h4>
                <p style={{ margin: '4px 0 0 0', fontSize: '13px', color: '#94a3b8', lineHeight: 1.4 }}>
                  {showAdminRoleConfirmModal
                    ? (i18n.language === 'en'
                        ? `Are you sure you want to make "${editingItem?.firstName} ${editingItem?.lastName}" an Administrator? They will be able to approve, edit, and manage all organization projects.`
                        : `"${editingItem?.firstName} ${editingItem?.lastName}" kullanıcısına Yönetici (Admin) yetkisi vermek istediğinize emin misiniz? Adminler tüm projeleri onaylayabilir, düzenleyebilir ve yönetebilir.`)
                    : (i18n.language === 'en'
                        ? `Are you sure you want to revoke Administrator role from "${editingItem?.firstName} ${editingItem?.lastName}"?`
                        : `"${editingItem?.firstName} ${editingItem?.lastName}" kullanıcısından Yönetici (Admin) yetkisini kaldırmak istediğinize emin misiniz?`)}
                </p>
              </div>
            </div>
            <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end', marginTop: '8px' }}>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                disabled={isUpdatingAdminRole}
                onClick={() => setShowAdminRoleConfirmModal(null)}
              >
                {t('actions.cancel', { ns: 'common' })}
              </Button>
              <Button
                type="button"
                variant="primary"
                size="sm"
                disabled={isUpdatingAdminRole}
                onClick={() => handleAdminRoleConfirm(showAdminRoleConfirmModal)}
              >
                {isUpdatingAdminRole
                  ? (i18n.language === 'en' ? 'Updating...' : 'Güncelleniyor...')
                  : (showAdminRoleConfirmModal
                      ? (i18n.language === 'en' ? 'Yes, Grant Admin Role' : 'Evet, Yönetici Yap')
                      : (i18n.language === 'en' ? 'Yes, Revoke Role' : 'Evet, Yetkiyi Kaldır'))}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------------------
// DELETE CONFIRMATION MODAL
// ----------------------------------------------------------------------
function ConfirmDeleteModal({
  itemName,
  itemType,
  onConfirm,
  onCancel,
}: {
  itemName: string;
  itemType: TabType;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  const { t, i18n } = useTranslation(['common']);
  const getTypeLabel = () => {
    if (i18n.language === 'en') {
      if (itemType === 'teams') return 'team';
      if (itemType === 'members') return 'member';
      return 'department';
    }
    if (itemType === 'teams') return 'ekibini';
    if (itemType === 'members') return 'kişisini';
    return 'departmanını';
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 1100,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'rgba(0, 0, 0, 0.7)',
        backdropFilter: 'blur(3px)',
        padding: 'var(--space-4)',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '440px',
          backgroundColor: '#1e293b',
          color: '#f8fafc',
          borderRadius: 'var(--radius-lg)',
          boxShadow: '0 20px 25px -5px rgba(0,0,0,0.5)',
          border: '1px solid #334155',
          padding: 'var(--space-5)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', color: '#ef4444', marginBottom: 'var(--space-3)' }}>
          <AlertTriangle className="w-6 h-6" />
          <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, margin: 0, color: '#f8fafc' }}>
            {i18n.language === 'en' ? 'Confirm Deletion' : 'Silme Onayı'}
          </h3>
        </div>

        <p style={{ fontSize: 'var(--font-size-sm)', color: '#cbd5e1', marginBottom: 'var(--space-4)', lineHeight: 1.5 }}>
          {i18n.language === 'en' ? (
            <>Are you sure you want to delete <strong style={{ color: '#ffffff' }}>"{itemName}"</strong> ({getTypeLabel()})? This action cannot be undone.</>
          ) : (
            <><strong style={{ color: '#ffffff' }}>"{itemName}"</strong> {getTypeLabel()} silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.</>
          )}
        </p>

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)' }}>
          <Button variant="secondary" onClick={onCancel} style={{ borderColor: '#475569', color: '#cbd5e1' }}>
            {t('actions.cancel', { ns: 'common' })}
          </Button>
          <Button
            variant="danger"
            onClick={onConfirm}
          >
            {t('actions.delete', { ns: 'common' })}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default AdminOrganizationPage;
