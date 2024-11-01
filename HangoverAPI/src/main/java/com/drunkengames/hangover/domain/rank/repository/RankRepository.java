package com.drunkengames.hangover.domain.rank.repository;

import com.drunkengames.hangover.domain.rank.dto.RankDto;
import com.drunkengames.hangover.domain.rank.dto.RankRequestDto;
import jakarta.persistence.PersistenceException;

import java.util.List;

/**
 * <pre>
 *     랭킹 관리 레포지토리 인터페이스
 * </pre>
 *
 * @author 박봉균
 * @since JDK17 Eclipse Temurin
 */

public interface RankRepository {
    void insertRank(RankRequestDto rankRequestDto) throws PersistenceException;
    List<RankDto> listRank() throws PersistenceException;
}
