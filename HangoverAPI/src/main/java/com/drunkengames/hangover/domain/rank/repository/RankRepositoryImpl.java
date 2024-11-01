package com.drunkengames.hangover.domain.rank.repository;

import com.drunkengames.hangover.domain.rank.dto.RankDto;
import com.drunkengames.hangover.domain.rank.dto.RankRequestDto;
import com.querydsl.core.types.Projections;
import com.querydsl.jpa.impl.JPAQueryFactory;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import jakarta.persistence.PersistenceException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Repository;

import java.util.List;

import static com.drunkengames.hangover.domain.rank.entity.QRank.rank;

/**
 * <pre>
 *     랭킹 관리 레포지토리 구현 클래스
 * </pre>
 *
 * @author 박봉균
 * @since JDK17 Eclipse Temurin
 */

@Repository
@RequiredArgsConstructor
public class RankRepositoryImpl implements RankRepository {
    @PersistenceContext
    private final EntityManager em;
    private final JPAQueryFactory queryFactory;

    @Override
    public void insertRank(RankRequestDto rankRequestDto) throws PersistenceException {
        em.persist(rankRequestDto.toEntity());
    }

    @Override
    public List<RankDto> listRank() throws PersistenceException {
        return queryFactory
                .select(Projections.constructor(RankDto.class,
                        rank.id,
                        rank.userNickname,
                        rank.finalMoney,
                        rank.finalDay))
                .from(rank)
                .orderBy(rank.finalMoney.desc())
                .limit(20)
                .fetch();
    }
}
